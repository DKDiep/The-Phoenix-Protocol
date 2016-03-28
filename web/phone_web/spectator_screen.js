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

// Used to interrupt the rendering
var keepRendering = false;

// Game variables
var playerShip;
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

    // You need to create a root container that will hold the scene you want to draw.
    stage = undefined;

    // Texture loader
    loader = undefined;

    // Holds the textures after the loading is done
    loadedResources = undefined;

    // Background Image
    bg = undefined;

    // Used to interrupt the rendering
    keepRendering = false;

    // Game variables
    playerShip = undefined;
    enemies = new Array();
    asteroids = new Array();

    // Controll data
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
    playerShip = new PIXI.Sprite(loadedResources.ship.texture);

    // Set center of mass atound center
    playerShip.anchor.x = 0.5;
    playerShip.anchor.y = 0.5;

    // Setup the position and scale of the ship
    playerShip.position.x = 0.5*renderer.width;
    playerShip.position.y = 0.5*renderer.height;

    // Add moving background
    bg = new PIXI.extras.TilingSprite(loadedResources.stars.texture,
                                        renderer.width, renderer.height);
    bg.position.x = 0;
    bg.position.y = 0;
    bg.tilePosition.x = 0;
    bg.tilePosition.y = 0;

    // Add ship and background to the scene we are building.
    stage.addChild(bg)
    stage.addChild(playerShip);

    // kick off the animation loop (defined below)
    keepRendering = true
    resize();
    renderUpdate();
}

// Render function
function renderUpdate() {
    // exit if we dont have to render anymore
    if(!keepRendering) {
        return
    }
    // start the timer for the next animation loop
    requestAnimationFrame(renderUpdate);

    // move background
    bg.tilePosition.y += 0.5

    // this is the main render call that makes pixi draw your container and its children.
    renderer.render(stage);
}

// Resizing function
function resize() {
    newHeight = $(window).height();
    newWidth = $(window).width();

    maxDim = Math.max(newWidth, newHeight);

    oldHeight = renderer.height;
    oldWidth = renderer.width;
    renderer.resize(newWidth, newHeight)
    bg.width = newWidth
    bg.height = newHeight
    for(i = 0; i < stage.children.length; i++) {
        sprite = stage.getChildAt(i);
        if (sprite != bg) {
            spriteResize(sprite);
        }
    }
}

// Sets the scale of the sprite based on a number of factors
function spriteResize(sprite) {
    // Reposition
    sprite.position.x = (sprite.position.x/oldWidth)*renderer.width;
    sprite.position.y = (sprite.position.y/oldHeight)*renderer.height;
    // Rescale
    spriteScale(sprite)
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
            if(sprite.spaceGameId == controlledEnemyId) {
                sprite.texture = loadedResources.hacked.texture
            }
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
