data = {
    "objects": [
        {
            "type": "ship",
            "position": {
                "x": 43,
                "y": 13
            }
        },
        {
            "type": "ship",
            "position": {
                "x": 1,
                "y": 1
            }
        },
        {
            "type": "debris",
            "position": {
                "x": 40,
                "y": 36
            },
            "size": 10
        },
        {
            "type": "debris",
            "position": {
                "x": 80,
                "y": 70
            },
            "size": 10
        },
        {
            "type": "asteroid",
            "position": {
                "x": 96,
                "y": 40
            }
        },
        {
            "type": "asteroid",
            "position": {
                "x": 16,
                "y": 73
            }
        },
        {
            "type": "asteroid",
            "position": {
                "x": 44,
                "y": 32
            }
        },
        {
            "type": "asteroid",
            "position": {
                "x": 14,
                "y": 39
            }
        },
        {
            "type": "asteroid",
            "position": {
                "x": 94,
                "y": 72
            }
        },
        {
            "type": "asteroid",
            "position": {
                "x": 14,
                "y": 22
            }
        },
        {
            "type": "asteroid",
            "position": {
                "x": 80,
                "y": 12
            }
        }
    ]
};
var numOfBigStars = 100;
var numOfSmallStars = 350;
var xSpeed = 0;
var ySpeed = 2;
var sStars = new Array();
var bStars = new Array();
var canvas = document.getElementById('canvas');
var ctx = canvas.getContext('2d');
var shiploaded = 0;
var asteroidloaded = 0;
var ship = new Image();
var asteroid = new Image();
var enemy = new Image();
var collectedResources = 0;
(function() {
    // resize the canvas to fill browser window dynamically
    window.addEventListener('resize', resizeCanvas, false);
    window.requestAnimationFrame(frame);
    initStars();
    resizeCanvas();

    ship.src = 'img/ship.png';
    asteroid.src = 'img/rock.png';
    enemy.src = 'img/enemy.png';

    ship.onload = function() {
      shiploaded = 1;
      resizeCanvas();
    };

    asteroid.onload = function() {
      asteroidloaded = 1;
      resizeCanvas();
    };

    $("#canvas").on("click", handleClick);

})();


function frame() {
  removeStars();
  moveStars();
  drawStuff();
  window.requestAnimationFrame(frame);
}

function getMouseX(e) {
  var x = e.pageX;
  x = 100*x / window.innerWidth;
  return x;
}
function getMouseY(e) {
  var x = e.pageY - 20; // minus 20 as currently there is text along the top.
  x = 100*x / window.innerHeight;
  return x;
}
function handleClick(e) {
  $.each( data.objects, function( key, object ) {
    if(Math.abs(object.position.x - getMouseX(e)) < 3 && Math.abs(object.position.y - getMouseY(e)) < 3) {
      var msg = { type: "", data: "" }
      switch(object.type) {
        case "ship":
          // Do nothing for clicking ships
          console.log("ship");
        break;
        case "debris":
          collectedResources += 2;
          msg.type = "ADD_RESOURCES";
          msg.data = 2;
          object.size -= 2;
          serverSocket.send(JSON.stringify(msg));

          if(object.size < 1) {
            ctx.beginPath();
            ctx.arc((object.position.x*window.innerWidth/100) + 7.5, (object.position.y*window.innerHeight/100) + 7.5, 15, 0, 2 * Math.PI, false);
            ctx.fillStyle = "#000";
            ctx.fill();

            data.objects.splice(key,1);
          }
        break
        case "asteroid":
          // Do nothing for clicking asteroids
        break
      }
    }
  });
}



function resizeCanvas() {
  canvas.width = window.innerWidth;
  canvas.height = window.innerHeight-10;

  drawStuff();
}
function drawStuff() {
  if(asteroidloaded && shiploaded) {
    ship.width = window.innerWidth/8;
    ship.height = ship.width*1.75;
    drawStars();

    ctx.drawImage(ship, (window.innerWidth-ship.width)/2, (window.innerHeight-ship.height)/2,   ship.width,  ship.height);

    // Draw ships and asteroids.
    $.each( data.objects, function( key, value ) {
      switch(value.type) {
        case "ship":
          ctx.drawImage(enemy, value.position.x * window.innerWidth/100, (value.position.y*window.innerHeight/100), 25, 25);
        break;
        case "debris":
          ctx.drawImage(asteroid, value.position.x*window.innerWidth/100, value.position.y*window.innerHeight/100, 15, 15);
        break
        case "asteroid":
          ctx.drawImage(asteroid, value.position.x*window.innerWidth/100, value.position.y*window.innerHeight/100, 30, 30);
        break
      }
    });
  }
}

function drawStars() {
  $.each( bStars, function( key, star ) {
    ctx.beginPath();
    ctx.arc(star.x * canvas.width, star.y * canvas.height, 1, 0, 2 * Math.PI, false);
    ctx.lineWidth = (canvas.height+canvas.width)/700;
    ctx.strokeStyle = '#fff';
    ctx.stroke();
  });
  $.each( sStars, function( key, star ) {
    ctx.beginPath();
    ctx.arc(star.x * canvas.width, star.y * canvas.height, 1, 0, 2 * Math.PI, false);
    ctx.lineWidth = (canvas.height+canvas.width)/1600;
    ctx.strokeStyle = '#fff';
    ctx.stroke();
  });
}
function removeStars() {
  $.each( bStars, function( key, star ) {
    ctx.beginPath();
    ctx.arc(star.x * canvas.width, star.y * canvas.height, 1, 0, 2 * Math.PI, false);
    ctx.lineWidth = (canvas.height+canvas.width)/350;
    ctx.strokeStyle = '#000';
    ctx.stroke();
  });
  $.each( sStars, function( key, star ) {
    ctx.beginPath();
    ctx.arc(star.x * canvas.width, star.y * canvas.height, 1, 0, 2 * Math.PI, false);
    ctx.lineWidth = (canvas.height+canvas.width)/800;
    ctx.strokeStyle = '#000';
    ctx.stroke();
  });
}
function moveStars() {
  for (i = 0; i < numOfBigStars; i++) {
    bStars[i].x += xSpeed / 10000;
    bStars[i].y += ySpeed / 10000;
    // Recycle stars
    if(bStars[i].y > 1) {
      bStars[i].y = 0;
    }
    if(bStars[i].x > 1) {
      bStars[i].x = 0;
    }
  }
  for (i = 0; i < numOfSmallStars; i++) {
    sStars[i].x += xSpeed / 20000;
    sStars[i].y += ySpeed / 20000;
    // Recycle stars
    if(sStars[i].y > 1) {
      sStars[i].y = 0;
    }
    if(sStars[i].x > 1) {
      sStars[i].x = 0;
    }
  }
}


function initStars() {
  for (i = 0; i < numOfBigStars; i++) {
    bStars.push({ x: Math.random(), y: Math.random() });
  }
  for (i = 0; i < numOfSmallStars; i++) {
    sStars.push({ x: Math.random(), y: Math.random() });
  }
}


function turnShip(n) {
  xSpeed = n/2;
  ySpeed = Math.abs(n);
  if(n == 0) {
    ySpeed = 1;
  }
}
