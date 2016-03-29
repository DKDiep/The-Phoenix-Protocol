// zoom level as percent of display pixels per game unit
var zoom = 0.002;

// detail of provided textures: that is pixels per game unit of length
var textureDetail = 3;

// renderer and scaling related variables
// Note: canvas renderer seems to perform better than WebGL one
// var renderer = new PIXI.autoDetectRenderer(10, 10);
var renderer;
var oldWidth;
var oldHeight;
var maxDim;

// You need to create a root container that will hold the scene you want to draw.
var stage;

// Texture loader
var loader;

// Holds the textures after the loading is done
var loadedResources;

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
var enemies = new Array();
var asteroids = new Array();

// Initiates the game
function startSpectatorScreen() {
    renderer = new PIXI.CanvasRenderer(10, 10);
    oldWidth = renderer.width;
    oldHeight = renderer.height;
    maxDim = Math.max(renderer.width, renderer.height);

    // The renderer will create a canvas element for you that you can then insert into the DOM.
    $("#spectatorScreen").append(renderer.view);

    // You need to create a root container that will hold the scene you want to draw.
    stage = new PIXI.Container();
    stage.interactive = true
    stage.mousedown = stage.touchstart = handleGeneralPress

    // Resize listener
    window.addEventListener("resize", function() {
        resize();
    });

    // Texture loader
    loader = new PIXI.loaders.Loader();
    loader.add("ship", "img/ship.png");
    loader.add("stars", "img/stars.png");
    loader.add("ast", "img/rock.png");
    loader.add("enm", "img/enemy.png");
    loader.add("hacked", "img/enemy_hacked.png");
    loader.add("tract_beam", "img/tractor_beam.png")

    // load the textures we need and initiate the rendering
    loader.load(function (loader, resources) {
        loadedResources = resources;
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

    stage = undefined;

    loader = undefined;

    loadedResources = undefined;

    bg = undefined;

    tractorBeam = undefined;

    keepRendering = false;

    enemyControllUpdate = undefined

    playerShip = undefined;
    controlledEnemySprite = undefined;
    enemies = new Array();
    asteroids = new Array();

    controlledEnemyId = 0;
    isControllingEnemy = false;
}

// Deals with movement
function handleGeneralPress(eventData) {
    screenX = eventData.data.originalEvent.pageX
    screenY = eventData.data.originalEvent.pageY

    // Invert coordinates to game space
    gameX = (screenX - (0.5*renderer.width))/(zoom*maxDim);
    gameY = (screenY - (0.5*renderer.height))/(zoom*maxDim);

    moveAction(gameX, gameY)
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

    // kick off the animation loop (defined below)
    keepRendering = true
    resize();
    renderUpdate();
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

// Render function
function renderUpdate() {
    // exit if we dont have to render anymore
    if(!keepRendering) {
        return
    }

    // start the timer for the next animation loop
    requestAnimationFrame(renderUpdate);

    // Update display information about controlled enemy if necessary
    if(enemyControllUpdate != undefined) {
        enemyControllUpdate()
        enemyControllUpdate = undefined
    }

    // Animate tracktor beam
    updateTractorBeam()

    // move background
    updateBackground()

    // this is the main render call that makes pixi draw your container and its children.
    renderer.render(stage);
}

// Animate tractor beam
function updateTractorBeam() {
    if(tractorBeam.target != undefined) {
        distance = distanceOfTwoSprites(tractorBeam, tractorBeam.target);
        angle = angleOfLine(tractorBeam, tractorBeam.target);
        tractorBeam.rotation = angle;
        tractorBeam.width = distance;
        tractorBeam.tilePosition.x -= 0.8;
    }
}

// Move background a bit
function updateBackground() {
    bg.tilePosition.y += 0.5
}

// Resizing function
function resize() {
    newHeight = $(window).height();
    newWidth = $(window).width();

    maxDim = Math.max(newWidth, newHeight);

    oldHeight = renderer.height;
    oldWidth = renderer.width;
    renderer.resize(newWidth, newHeight);

    // Recale background
    bg.width = newWidth;
    bg.height = newHeight;

    // Reposition and rescale tractor beam
    spriteReposition(tractorBeam);
    // Thinkness of beam in game length units
    var beamThickness = 7;
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
        spriteReposition(sprite);
        spriteScale(sprite);
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
    sprite.position.y = (0.5*renderer.height) + (y*zoom*maxDim);
}

// Update the objects based on received data
function updateSprites(data) {
    // Update asteroids
    var newTmp = new Array();
    var toAdd = new Array();
    // Flag to add and change existing
    for (id in data.asts) {
        var ast = data.asts[id]
        var sprite = asteroids[ast.id]
        if(sprite == undefined) {
            toAdd.push(ast)
        } else {
            spritePosition(sprite, ast.x, ast.y);
            // TODO: implement rotation
            // sprite.rotation = 12
            newTmp[ast.id] = sprite;
            delete asteroids[ast.id];
        }
    }
    // Remove those that didn't get an update
    for (id in asteroids) {
        stage.removeChild(asteroids[id]);
    }
    // Add new ones
    for (id in toAdd) {
        var ast = toAdd[id];
        var newAst = new PIXI.Sprite(loadedResources.ast.texture);
        newAst.anchor.x = 0.5;
        newAst.anchor.y = 0.5;
        spritePosition(newAst, ast.x, ast.y);
        spriteScale(newAst);
        newTmp[ast.id] = newAst;
        stage.addChild(newAst);
    }
    // Finalise by setting the asteroid list
    asteroids = newTmp
    //-----------------------------------------------------------------------//
    // Update enemies
    var newTmp = new Array();
    var toAdd = new Array();
    // Flag to add and change existing
    for (id in data.enms) {
        var enm = data.enms[id]
        var sprite = enemies[enm.id]
        if(sprite == undefined) {
            toAdd.push(enm)
        } else {
            spritePosition(sprite, enm.x, enm.y);
            // TODO: implement rotation
            // sprite.rotation = 12
            newTmp[enm.id] = sprite;
            delete enemies[enm.id];
        }
    }
    // Remove those that didn't get an update
    for (id in enemies) {
        stage.removeChild(enemies[id]);
    }
    // Add new ones
    for (id in toAdd) {
        var enm = toAdd[id]
        newEnm = new PIXI.Sprite(loadedResources.enm.texture);
        newEnm.spaceGameId = enm.id
        newEnm.anchor.x = 0.5
        newEnm.anchor.y = 0.5
        spritePosition(newEnm, enm.x, enm.y);
        // TODO: implement rotation
        // sprite.rotation = 12
        spriteScale(newEnm);
        newEnm.interactive = newEnm.buttonMode = true;
        newEnm.mousedown = newEnm.touchstart = function (eventData) {
            actionOnEnemy(eventData.target.spaceGameId)
            // Prevents the triggering of the move event
            eventData.stopPropagation()
        };
        newTmp[enm.id] = newEnm;
        stage.addChild(newEnm);
    }
    // Finalise by setting the asteroid list
    enemies = newTmp
}

// Set the target of the tractor beam
function enableTractorBeam(target) {
    tractorBeam.target = target;
}

function disableTractorBeam() {
    tractorBeam.target = undefined;
    tractorBeam.width = 0;
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

function findControlledEnemy() {
    for (id in enemies) {
        var sprite = enemies[id]
        if(sprite.spaceGameId == controlledEnemyId) {
            return sprite
        }
    }
}
