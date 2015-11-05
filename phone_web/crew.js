var MAX_AMMO = 20
var AMMO_RECHARGE_RATE = 500
var ammoRechargeTimer
var currentAmmunition = 0;

//Sets the ammunition to the specified ammount and restarts the recharge
//timer.
function updateAmmo(ammo) {
    clearInterval(ammoRechargeTimer)
    for (i = 0; i < ammo ; i++) {
        $("#ammoSegment"+i).addClass("filled")
    }
    for (i = ammo; i < MAX_AMMO ; i++) {
        $("#ammoSegment"+i).removeClass("filled")
    }
    currentAmmunition = ammo
    ammoRechargeTimer = setInterval(rechargeAmmo, AMMO_RECHARGE_RATE)
}

//Fills the next empty ammo segment
function rechargeAmmo() {
    if (currentAmmunition < MAX_AMMO) {
        $("#ammoSegment"+currentAmmunition).addClass("filled")
        currentAmmunition++;
    }
}
