function updateAmmo(ammo) {
    max = 20
    border = '<div class="border bar"></div>\n'
    filled = '<div class="item bar filled"></div>\n'
    empty = '<div class="item bar"></div>'
    bars = ""
    for (i = 0; i < ammo; i++) {
        bars += border;
        bars += filled;
    }
    for (i = 0; i < max-ammo; i++) {

        bars += border;
        bars += empty;
    }
    bars += border;
    $("#ammoBar").html(bars)
}
