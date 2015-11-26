var shiploaded = 0;
var asteroidloaded = 0;
var then = Date.now();
var ship = new Image();
var asteroid = new Image();
var enemy = new Image();

ship.src = 'img/ship.png';
asteroid.src = 'img/rock.png';
enemy.src = 'img/enemy.png';

ship.onload = function() {
  shiploaded = 1;
};
asteroid.onload = function() {
  asteroidloaded = 1;
};


function initCanvas() {
  canvas = document.getElementById('canvas');
  ctx = canvas.getContext('2d');
  // resize the canvas to fill browser window dynamically
  window.addEventListener('resize', resizeCanvas, false);
  window.requestAnimationFrame(frame);
  $("#canvas").on("click", handleClick);
  resizeCanvas();
}

function frame() {
  window.requestAnimationFrame(frame);
  var now = Date.now();
  var delta = now - then;
  if (delta > 1000/fps) {
      then = now - (delta % (1000/fps));

      removeStars();
      moveStars();
      drawObjects();
  }
}

function resizeCanvas() {
  canvas.width = window.innerWidth;
  canvas.height = window.innerHeight-10;

  drawObjects();
}
function drawObjects() {
  var x;
  var y;

  if(asteroidloaded && shiploaded) {
    ship.width = window.innerWidth/8;
    ship.height = ship.width*2.5;

    drawStars();
    ctx.drawImage(ship, (window.innerWidth-ship.width)/2, (window.innerHeight-ship.height)/2,   ship.width,  ship.height);

    // Draw ships and asteroids.
    $.each( data.objects, function( key, value ) {
      x = value.position.x * window.innerWidth/100;
      y = value.position.y*window.innerHeight/100;
      switch(value.type) {
        case "ship":
          clearArea(x, y, 50);
          ctx.drawImage(enemy, x, y, 25, 25);
        break;
        case "debris":
          clearArea(x, y, 50);
          ctx.drawImage(asteroid, x, y, 15, 15);
        break
        case "asteroid":
          clearArea(x, y, 50);
          ctx.drawImage(asteroid, x, y, 30, 30);
        break
      }
    });
  }
}





function clearArea(x, y, size) {
  ctx.beginPath();
  ctx.arc(x, y, size, 0, 2 * Math.PI, false);
  ctx.fillStyle = "#000";
  ctx.fill();
}
