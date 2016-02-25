$(document).ready(function(){
  function refreshData(){
        $.getJSON("json", function(result){
      var json_obj = $.parseJSON(result);

      console.log(json_obj.shipResources);

      if(typeof json_obj === 'undefined') {
        console.log("Error retrieving data");
      } else {
        $('#playerStats').html("");
          $.each(json_obj.playerscores, function(index, score) {
              $('#playerStats').append('<div class="player"><div class="player_icon glyphicon glyphicon-user"></div><div class="player_name">Sporcle</div><div class="player_score">'+score+'</div></div>');
          });
          $("#shipHealth").html(json_obj.shipHealth);
          $("#shipResources").html(json_obj.shipResources);


          /*$('body').append($('<div>', {
              text: "Total Collected Resources: " + json_obj.totalShipResources
          }));*/
      }
        });
    }
    refreshData();
    setInterval(refreshData,5000)
});
