var numOfBigStars = 100;
var numOfSmallStars = 350;
var sStars = new Array();
var bStars = new Array();

function drawStars() {
  $.each( bStars, function( key, star ) {
    ctx.beginPath();
    ctx.arc(star.x * canvas.width, star.y * canvas.height, (canvas.height+canvas.width)/700, 0, 2 * Math.PI, false);
    ctx.fillStyle = "#fff";
    ctx.fill();
  });
  $.each( sStars, function( key, star ) {
    ctx.beginPath();
    ctx.arc(star.x * canvas.width, star.y * canvas.height, (canvas.height+canvas.width)/1600, 0, 2 * Math.PI, false);
    ctx.fillStyle = "#aaa";
    ctx.fill();
  });
}

function removeStars() {
  $.each( bStars, function( key, star ) {
    ctx.beginPath();
    ctx.arc(star.x * canvas.width, star.y * canvas.height, (canvas.height+canvas.width)/350, 0, 2 * Math.PI, false);
    ctx.fillStyle = "#000";
    ctx.fill();
  });
  $.each( sStars, function( key, star ) {
    ctx.beginPath();
    ctx.arc(star.x * canvas.width, star.y * canvas.height, (canvas.height+canvas.width)/800, 0, 2 * Math.PI, false);
    ctx.fillStyle = "#000";
    ctx.fill();
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
