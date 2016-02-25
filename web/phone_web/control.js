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
    if(Math.abs(object.position.x - getMouseX(e)) < 5 && Math.abs(object.position.y - getMouseY(e)) < 5) {
      var msg = { type: "", data: "" }
      switch(object.type) {
        case "ship":
          // Do nothing for clicking ships
          console.log("ship");
        break;
        case "debris":
          vibrate();
          collectedResources += 2;
          msg.type = "ADD_RESOURCES";
          msg.data = 2;
          object.size -= 2;
          serverSocket.send(JSON.stringify(msg));
          updateScore();
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

function vibrate() {
  navigator.vibrate = navigator.vibrate || navigator.webkitVibrate || navigator.mozVibrate || navigator.msVibrate;
  if (navigator.vibrate) {
    navigator.vibrate(100);
  }
}
