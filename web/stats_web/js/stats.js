$(document).ready(function(){
  function refreshData(){
        $.getJSON("json", function(result){
      var json_obj = $.parseJSON(result);//parse JSON
        console.log(json_obj.shipResources);
      if(typeof json_obj === 'undefined'){
        $('body').append($('<div>', {
              text: "Error retrieving data"
          }));
      }
      else{
        document.body.innerHTML = ""
          $.each(json_obj.playerscores, function(index, element) {
              $('body').append($('<div>', {
                  text: "Player " + index + " Score: " + element
              }));
          });
          $('body').append($('<div>', {
              text: "Ship Health: " + json_obj.shipHealth
          }));
          $('body').append($('<div>', {
              text: "Current Ship Resources: " + json_obj.shipResources
          }));
          $('body').append($('<div>', {
              text: "Total Collected Resources: " + json_obj.totalShipResources
          }));
      }
        });
    }
    setInterval(refreshData,5000)
});
