
var Game = {

    Init: function () {
        var proxy = new GameHub('GameHub', window.userid, '');
        proxy.on('Online', this.Online.bind(this));
        proxy.on('Offline', this.Offline.bind(this));

        proxy.start();
    },


    // Server Callbacks ----------------------------------------
    Online: function () {
        console.log('server is online');

        $('#Notification > .item').hide();
        $('#Notification > .item.waiting_opponent').show();
    },

    Offline: function () {
        console.log('server is offline');
    }
}

Game.Init();
