
var Game = {

    Init: function () {
        var proxy = new GameHub('GameHub', window.userid, '');

        proxy.start();
    }

}

Game.Init();
