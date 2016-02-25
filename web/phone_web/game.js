var data = {
      "objects": []
    };

var canvas;
var ctx;
var fps = 30;


var xSpeed = 0;
var ySpeed = 10;
var collectedResources = 0;







function startGame() {
  initCanvas();
  initStars();
}









function updateScore() {
  var score = collectedResources;
  $("#score").html(score);
}

function turnShip(n) {
  xSpeed = n/2;
  ySpeed = Math.abs(n);
  if(n == 0) {
    ySpeed = 1;
  }
}
