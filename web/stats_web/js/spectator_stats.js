$(document).ready(function(){
    function refreshData(){
        $.getJSON("/get_spectator_data", function(result){
            if(typeof result === 'undefined') {
                console.log("Error retrieving data");
            } else {
                $('#spectator_stats').html("");
                $.each(result.spectators, function(index, player) {
                    $('#spectator_stats').append('<div class="spectotor_box"><div class="name">'+player.name+'</div><div class="score">'+player.score+'</div></div>');
                });
            }
        });
    }
    refreshData();
    setInterval(refreshData,3000)
});
