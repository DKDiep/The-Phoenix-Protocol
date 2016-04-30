// --Constants
// zoom level as percent of display pixels per game unit
var zoom = 0.003;
var frameRate = 25;
var touchTargetSizeFactor = 0.16;
var touchTargetAlpha = 0.9;

// detail of provided textures: that is pixels per game unit of length
var textureDetail = 528/49;

// --Variables
// renderer and scaling related variables
// Note: canvas renderer seems to perform better than WebGL one
// var renderer = new PIXI.autoDetectRenderer(10, 10);
var renderer;
var oldWidth;
var oldHeight;
var maxDim;
var minDim;

// You need to create a root container that will hold the scene you want to draw.
var stage;
// Layers
var beamLayer;
var enemyLayer;
var enemyNamesLayer;
var asteroidLayer;
var hackingGameLayer;
var uiLayer;
var tutorialLayer;

// Texture loader
var loader;

// Holds the textures after the loading is done
var loadedResources;
var textures;

// Background Image
var bg;

// Tractor beam sprite
var tractorBeam;

// Used to interrupt the rendering
var keepRendering = false;

// Holds a function that needs to be called on first available update
// Deals with a change in controlled enemy
var enemyControllUpdate;

// Game variables
var playerShip;
var controlledEnemySprite;
var attackedEnemySprite;
var enemies = new Array();
var asteroids = new Array();
var touchTargets = new Array();

var tutorialPrompts = new Array();

var playerScore = 0;

// Initiates the game
function startSpectatorScreen() {
    renderer = new PIXI.CanvasRenderer(10, 10);
    oldWidth = renderer.width;
    oldHeight = renderer.height;
    maxDim = Math.max(renderer.width, renderer.height);
    minDim = Math.min(renderer.width, renderer.height);

    // The renderer will create a canvas element for you that you can then insert into the DOM.
    $("#spectatorScreen").append(renderer.view);

    initLayers()

    // Resize listener
    window.addEventListener("resize", function() {
        resize();
    });

    // Texture loader
    loader = new PIXI.loaders.Loader();
    loader.add("ship", "img/ship.png");
    loader.add("stars", "img/stars2.jpg"); // http://s3.amazonaws.com/everystockphoto/fspid31/31/20/26/1/tileable-classic-nebula-3120261-o.jpg
    loader.add("ast1", "img/asteroid11.png");
    loader.add("ast2", "img/asteroid22.png");
    loader.add("ast3", "img/asteroid33.png");
    loader.add("enm", "img/firefly.png");
    loader.add("hacked", "img/hacked_overlay.png");
    loader.add("controlled", "img/controlled_overlay.png");
    loader.add("tract_beam", "img/tractor_beam.png");
    loader.add("target_ray", "img/target_ray.png");
    loader.add("finger_target", "img/finger_target.png");
    loader.add("tutorial_1", "img/tutorial1.png");

    // load the textures we need and initiate the rendering
    loader.load(function (loader, resources) {
        loadedResources = resources;
        initTextureObject(resources);
        // init rendering
        init();
    });
}

// Clear states before exiting the spectator game
function finaliseSpectatorScreen() {
    renderer = undefined;
    oldWidth = undefined;
    oldHeight = undefined;
    maxDim = undefined;
    minDim = undefined;

    stage = undefined;
    beamLayer = undefined;
    enemyLayer = undefined;
    enemyNamesLayer = undefined;
    asteroidLayer = undefined;
    hackingGameLayer = undefined;
    uiLayer = undefined;
    tutorialLayer = undefined;

    loader = undefined;

    loadedResources = undefined;
    textures = undefined;

    bg = undefined;

    tractorBeam = undefined;

    keepRendering = false;

    enemyControllUpdate = undefined

    playerShip = undefined;
    controlledEnemySprite = undefined;
    attackedEnemySprite = undefined;
    enemies = new Array();
    asteroids = new Array();
    touchTargets = new Array();
    tutorialPrompts = new Array();

    controlledEnemyId = 0;
    isControllingEnemy = false;

    playerScore = 0;
}

// Handle specific updates
function updateSpectator(msg) {
    switch(msg.type) {
        case "SCORE":
            playerScore = msg.data
            if(uiLayer != undefined && uiLayer.children[1] != undefined) {
                uiLayer.children[1].text = "Score: " + playerScore;
            }
            break;
        case "OBJ":
            updateSprites(msg.data);
            break;
    }
}

// Deals with movement
function handleGeneralPress(eventData) {
    // If we click somewhere that is not an enemy
    // then we are no longer holding an enemy
    // so our hack progress will decrement
    if (!isControllingEnemy) {
        setHeld(false)
    }

    var screenX;
    var screenY;

    if (eventData.data.originalEvent.touches != undefined) {
        screenX = eventData.data.originalEvent.touches[0].pageX
        screenY = eventData.data.originalEvent.touches[0].pageY
    } else {
        screenX = eventData.data.originalEvent.pageX
        screenY = eventData.data.originalEvent.pageY
    }

    // Invert coordinates to game space
    gameX = (screenX - (0.5*renderer.width))/(zoom*maxDim);
    // Negate Y to revert from rendering orientation
    gameY = -(screenY - (0.5*renderer.height))/(zoom*maxDim);

    moveAction(gameX, gameY)
}

// Event for doing an action on tand enemy
function enemyActionEvent(eventData) {
    actionOnEnemy(eventData.target.enemy)
    // Prevents the triggering of the move event
    eventData.stopPropagation()
}

// Place player ship in middle and add moving background
function init() {
    // Rendering order is based on the order in which things were added
    // Add moving background
    initBackground(loadedResources);
    // Define tractor beam
    initTractorBeam(loadedResources);
    // Add player ship
    initPlayerShip(loadedResources);
    // Add tutorial prompts
    initTutorialPrompts(loadedResources);

    // Init User interface
    initUi();
    stage.addChild(beamLayer);
    stage.addChild(enemyLayer);
    stage.addChild(enemyNamesLayer);
    stage.addChild(asteroidLayer);
    stage.addChild(hackingGameLayer);

    // Only add the tutorial layer and ui layer if we are not connecting automatically,
    // i.e. on the stats screen
    if(window.location.hash.substr(1) != "auto") {
        stage.addChild(uiLayer);
        stage.addChild(tutorialLayer);
    }


    // kick off the animation loop (defined below)
    keepRendering = true
    resize();
    // Use this event to check for if we have left a hacking target
    $("#spectatorScreen").on("touchmove", function (event){
        var touch = event.originalEvent.touches[0];
        hackProgressCheck(touch.pageX, touch.pageY);
    });

    renderUpdate();
}

// Inits the object that holds the arrays of different types of textures
function initTextureObject(resources) {
    textures = {}
    // Asteroid variants
    textures.asteroids = new Array();
    textures.asteroids.push(resources.ast1.texture)
    textures.asteroids.push(resources.ast2.texture)
    textures.asteroids.push(resources.ast3.texture)
}

function initTutorialPrompts(resources) {
    tutorialPrompts[0] = new PIXI.Sprite(resources.tutorial_1.texture);
    tutorialPrompts[0].anchor.x = 1;
    resizeTutorialPrompt(tutorialPrompts[0]);
    tutorialLayer.addChild(tutorialPrompts[0]);
}

function resizeTutorialPrompt(prompt) {
    prompt.height = 0.15*Math.sqrt(maxDim*minDim);
    prompt.x = renderer.width;
    var mul = prompt.height/prompt.texture.height
    prompt.width = mul*prompt.texture.width;
}

// Initialises the grouping layers
function initLayers() {
    // You need to create a root container that will hold the scene you want to draw.
    stage = new PIXI.Container();
    stage.interactive = true
    stage.mousedown = stage.touchstart = handleGeneralPress
    stage.mouseup = stage.touchend = leftHackingTarget
    beamLayer = new PIXI.Container();
    asteroidLayer = new PIXI.Container();
    enemyLayer = new PIXI.Container();
    enemyNamesLayer = new PIXI.Container();
    hackingGameLayer = new PIXI.Container();
    uiLayer = new PIXI.Container();
    tutorialLayer = new PIXI.Container();
}

function initBackground(resources) {
    bg = new PIXI.extras.TilingSprite(resources.stars.texture,
                                        renderer.width, renderer.height);
    bg.position.x = 0;
    bg.position.y = 0;
    bg.tilePosition.x = 0;
    bg.tilePosition.y = 0;

    stage.addChild(bg);
}

function initTractorBeam(resources) {
    tractorBeam = new PIXI.extras.TilingSprite(resources.tract_beam.texture, 1, 1);
    tractorBeam.anchor.y = 0.5;

    tractorBeam.position.x = 0.5*renderer.width;
    tractorBeam.position.y = 0.5*renderer.height;

    tractorBeam.tilePosition.x = 0;
    tractorBeam.tilePosition.y = 0;

    disableTractorBeam();

    stage.addChild(tractorBeam);
}

function initPlayerShip(resources) {
    playerShip = new PIXI.Sprite(loadedResources.ship.texture);

    // Set center of mass atound center
    playerShip.anchor.x = 0.5;
    playerShip.anchor.y = 0.5;

    // Setup the position and scale of the ship
    playerShip.position.x = 0.5*renderer.width;
    playerShip.position.y = 0.5*renderer.height;

    stage.addChild(playerShip);
}

function initUi() {
    var name = new PIXI.Text("User: " + playerName, {font : '2em XoloniumBold', fill : 0xd7e2ed, align : 'left'});
    name.position.x = 5;
    var score = new PIXI.Text("Score: " + playerScore, {font :'2em XoloniumBold', fill : 0xd7e2ed, align : 'left'});
    score.position.x = 5;
    score.position.y = name.position.y + name.height;
    uiLayer.addChild(name);
    uiLayer.addChild(score);
}

// Render function
function renderUpdate() {
    var start = new Date().getTime();
    // exit if we dont have to render anymore
    if(!keepRendering) {
        return
    }

    // Update display information about controlled enemy if necessary
    if(enemyControllUpdate != undefined) {
        enemyControllUpdate()
    }

    // Update the hacking values
    updateHacking();

    // Animate tracktor beam
    updateTractorBeam();

    updateTargetRays();

    // move background
    updateBackground();

    // this is the main render call that makes pixi draw your container and its children.
    renderer.render(stage);
    var end = new Date().getTime();
    var timeTaken = end-start;
    var maxTimePerFrame = 1000/frameRate
    setTimeout(renderUpdate, Math.max(maxTimePerFrame - timeTaken, 1))
}

// Animate tractor beam
function updateTractorBeam() {
    if(tractorBeam.target != undefined) {
        distance = distanceOfTwoSprites(tractorBeam, tractorBeam.target);
        angle = angleOfLine(tractorBeam, tractorBeam.target);
        tractorBeam.rotation = angle;
        // REQ_HACK_PROGRESS and hackProgress are global variables defined in hacking_game.js
        tractorBeam.width = distance * (hackProgress / REQ_HACK_PROGRESS);
        tractorBeam.tilePosition.x -= 0.8;
    }
}

function updateTargetRays() {
    for (id in enemies) {
        var enemy = enemies[id];
        var targetSprite = enemies[enemy.targetId]
        if(targetSprite != undefined) {
            distance = distanceOfTwoSprites(enemy.targetRay, targetSprite);
            angle = angleOfLine(enemy.targetRay, targetSprite);
            enemy.targetRay.rotation = angle;
            enemy.targetRay.width = distance;
            enemy.targetRay.tilePosition.x += 2;
        } else {
            enemy.targetRay.width = 0;
        }
    }
}

// Move background a bit
function updateBackground() {
    bg.tilePosition.y += 0.5
}

// Resizing function
function resize() {
    if(!keepRendering) {
        return
    }
    newHeight = $(window).height();
    newWidth = $(window).width();

    maxDim = Math.max(newWidth, newHeight);
    minDim = Math.min(newWidth, newHeight);

    oldHeight = renderer.height;
    oldWidth = renderer.width;
    renderer.resize(newWidth, newHeight);

    // Recale background
    bg.width = newWidth;
    bg.height = newHeight;

    // Reposition and rescale tractor beam
    spriteReposition(tractorBeam);
    // Thinkness of beam in game length units
    var beamThickness = 5;
    tractorBeam.height = beamThickness*zoom*maxDim;

    // Reposition and rescale game objects
    spriteReposition(playerShip);
    spriteScale(playerShip);
    for (id in asteroids) {
        var sprite = asteroids[id];
        spriteReposition(sprite);
        spriteScale(sprite);
    }
    for (id in enemies) {
        var sprite = enemies[id];
        var rayThickness = 3;
        sprite.targetRay.height = rayThickness*zoom*maxDim;
        spriteReposition(sprite);
        spriteScale(sprite);
    }
    for (id in touchTargets) {
        var sprite = touchTargets[id];
        sprite.height = sprite.width = touchTargetSizeFactor*maxDim
    }
    for (id in tutorialPrompts) {
        resizeTutorialPrompt(tutorialPrompts[id]);
    }
}

// Sets the scale of the sprite based on a number of factors
function spriteReposition(sprite) {
    sprite.position.x = (sprite.position.x/oldWidth)*renderer.width;
    sprite.position.y = (sprite.position.y/oldHeight)*renderer.height;
}

// Scales the sprite
function spriteScale(sprite) {
    imWidth = sprite.texture.width;
    imHeight = sprite.texture.height;
    sprite.width = (imWidth/textureDetail)*zoom*maxDim;
    sprite.height = (imHeight/textureDetail)*zoom*maxDim;
}

// positions around the centre
function spritePosition(sprite, x, y) {
    sprite.position.x = (0.5*renderer.width) + (x*zoom*maxDim);
    // Invert Y to put object in rendering orientation
    sprite.position.y = (0.5*renderer.height) + ((-y)*zoom*maxDim);
}

// Update the objects based on received data
function updateSprites(data) {
    if (!keepRendering) {
        return
    }
    // Update asteroids
    updateAsteroids(data.asts)
    // Update enemies
    updateEnemies(data.enms)
}

// Set the target of the tractor beam
function enableTractorBeam(target) {
    tractorBeam.target = target;
}

function disableTractorBeam() {
    if(tractorBeam != undefined) {
        tractorBeam.target = undefined;
        tractorBeam.width = 0;
    }
}

// Returns true when the tractor beam is enabled
// Otherwise, returns false
function isTractorBeamEnabled() {
    return tractorBeam.target != undefined
}

function angleOfLine(origin, end) {
    var deltaX = end.position.x - origin.position.x;
    var deltaY = end.position.y - origin.position.y;
    return Math.atan2(deltaY, deltaX);
}

function distanceOfTwoSprites(a, b) {
    var xDiff = a.position.x - b.position.x;
    var yDiff = a.position.y - b.position.y;
    return Math.sqrt(xDiff*xDiff + yDiff*yDiff);
}

function findEnemyWithID(enemyID) {
    for (id in enemies) {
        var sprite = enemies[id]
        if(sprite.spaceGameId == enemyID) {
            return sprite
        }
    }
}

function findControlledEnemy() {
    for (id in enemies) {
        var sprite = enemies[id]
        if(sprite.spaceGameId == controlledEnemyId) {
            return sprite
        }
    }
}

// Update asteroid visuals based on received data
function updateAsteroids(astData) {
    var newTmp = new Array();
    var toAdd = new Array();
    // Flag to add and change existing
    for (id in astData) {
        var ast = astData[id]
        var sprite = asteroids[ast.id]
        if(sprite == undefined) {
            toAdd.push(ast)
        } else {
            updateAsteroid(sprite, ast)
            newTmp[ast.id] = sprite;
            delete asteroids[ast.id];
        }
    }
    // Remove those that didn't get an update
    for (id in asteroids) {
        asteroidLayer.removeChild(asteroids[id]);
    }
    // Add new ones
    for (id in toAdd) {
        var ast = toAdd[id];
        var newAst = newAsteroid(ast)
        newTmp[ast.id] = newAst;
        asteroidLayer.addChild(newAst);
    }
    // Finalise by setting the asteroid list
    asteroids = newTmp
}

// Create new astroid sprite
function newAsteroid(astData) {
    var texture = textures.asteroids[Math.floor(Math.abs(astData.id)/18) % textures.asteroids.length];
    var newAst = new PIXI.Sprite(texture);
    newAst.alpha = astData.alpha
    newAst.anchor.x = 0.5;
    newAst.anchor.y = 0.5;
    spritePosition(newAst, astData.x, astData.y);
    spriteScale(newAst);

    return newAst
}

// Update asteroid sprite
function updateAsteroid(asteroid, astData) {
    spritePosition(asteroid, astData.x, astData.y);
    asteroid.alpha = astData.alpha
}

// Updates the enemy sprites based on received data
function updateEnemies(enemyData) {
    var newTmp = new Array();
    var toAdd = new Array();
    // Flag to add and change existing
    for (id in enemyData) {
        var enm = enemyData[id]
        var sprite = enemies[enm.id]
        if(sprite == undefined) {
            toAdd.push(enm)
        } else {
            updateEnemy(sprite, enm)
            newTmp[enm.id] = sprite;
            delete enemies[enm.id];
        }
    }
    // Remove those that didn't get an update
    for (id in enemies) {
        var enemy = enemies[id];
        beamLayer.removeChild(enemy.targetRay);
        hackingGameLayer.removeChild(touchTargets[enemy.spaceGameId]);
        enemyNamesLayer.removeChild(enemy.textObj);
        delete touchTargets[enemy.spaceGameId];
        enemyLayer.removeChild(enemy);
    }
    // Add new ones
    for (id in toAdd) {
        var enm = toAdd[id]
        newEnm = newEnemy(enm)
        newTmp[enm.id] = newEnm;
        enemyLayer.addChild(newEnm);
    }
    // Finalise by setting the asteroid list
    enemies = newTmp
}

// Creates an enemy sprite based on received data
function newEnemy(enmData) {
    var newEnm = new PIXI.Sprite(loadedResources.enm.texture);
    newEnm.spaceGameId = enmData.id
    newEnm.anchor.x = 0.5
    newEnm.anchor.y = 0.5
    spritePosition(newEnm, enmData.x, enmData.y);
    newEnm.rotation = -enmData.rot
    spriteScale(newEnm);
    newEnm.textObj = generateTextBox(newEnm);
    newEnm.targetRay = generateTargetRay(newEnm);
    newEnm.isHacked = enmData.isHacked
    if(newEnm.isHacked) {
        newEnm.targetId = enmData.targetId;
        if(newEnm.spaceGameId != controlledEnemyId) {
            setName(newEnm.textObj, enmData.name);
            var overlay = new PIXI.Sprite(loadedResources.hacked.texture);
            overlay.anchor.x = 0.5
            overlay.anchor.y = 0.5
            newEnm.addChild(overlay)
        }
    }
    newEnm.touchTarget = generateTouchTarget(newEnm);

    return newEnm
}

function generateTargetRay(enemy) {
    var targetRay = new PIXI.extras.TilingSprite(loadedResources.target_ray.texture, 1, 1);
    targetRay.anchor.y = 0.5;

    targetRay.tilePosition.x = 0;
    targetRay.tilePosition.y = 0;

    var rayThickness = 3;
    targetRay.height = rayThickness*zoom*maxDim;

    targetRay.position = enemy.position;
    targetRay.width = 0;
    beamLayer.addChild(targetRay);

    return targetRay;
}

function generateTextBox(enemy) {
    textBox = new PIXI.Text("",{font : '1.5em XoloniumBold', fill : 0x00ff00, align : 'center'});
    textBox.position = enemy.position
    textBox.anchor.x = 0.5
    textBox.anchor.y = 1.7
    enemyNamesLayer.addChild(textBox);
    return textBox;
}

function setName(textObj, userName) {
    if(userName.length > 7) {
        name = userName.substring(0, 7) + "..."
    } else {
        name = userName
    }
    textObj.text = name
}

// Generate a target for the specified enemy
function generateTouchTarget(enemy) {
    var target = new PIXI.Sprite(loadedResources.finger_target.texture);
    target.anchor.x = 0.5;
    target.anchor.y = 0.5;
    target.position = enemy.position
    target.alpha = 0;
    target.width = target.height = touchTargetSizeFactor*maxDim;
    target.interactive = target.buttonMode = true;
    target.enemy = enemy;
    target.mousedown = target.touchstart = function (eventData) {
        canCheckBounds = false;
        setTimeout(function() {canCheckBounds = true;}, 250); // Nasty hack
        actionOnEnemy(eventData.target.enemy)
        // Prevents the triggering of the move event
        eventData.stopPropagation()
    };
    target.mouseup = target.touchend = leftHackingTarget
    target.mouseout = leftHackingTarget;
    hackingGameLayer.addChild(target);
    touchTargets[enemy.spaceGameId] = target;

    return target;
}

// Set interaction property for all touch targets, takes boolean
function setTouchTargetsInteraction(booley) {
    for (id in touchTargets) {
        var target = touchTargets[id]
        target.interactive = booley;
    }
}

// Updates an enemy sprite based on data
function updateEnemy(enemy, enmData) {
    spritePosition(enemy, enmData.x, enmData.y);
    enemy.rotation = -enmData.rot
    enemy.isHacked = enmData.isHacked
    if(enemy.isHacked) {
        enemy.targetId = enmData.targetId;
        enemy.touchTarget.interactive = false
        if(enemy.spaceGameId != controlledEnemyId) {
            setName(enemy.textObj, enmData.name);
            if(!enemy.hasOverlay) {
                enemy.hasOverlay = true
                var overlay = new PIXI.Sprite(loadedResources.hacked.texture);
                overlay.anchor.x = 0.5
                overlay.anchor.y = 0.5
                enemy.addChild(overlay)
            }
        }
    }
}
