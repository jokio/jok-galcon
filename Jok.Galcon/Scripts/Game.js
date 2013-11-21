
var Game = {
    stage : undefined,
    gameLayer: undefined,
    planets: [],
    Game: this,
    currentPlayerGroupID: undefined,
    colors: ['gray', 'green', 'yellow'],
    selectedPlanet: undefined,
    Init: function () {
        var proxy = new GameHub('GameHub', jok.config.sid, jok.config.channel);
        proxy.on('Online', this.Online.bind(this));
        proxy.on('Offline', this.Offline.bind(this));
        proxy.on('PlayerMove', this.PlayerMove.bind(this));
        proxy.on('TableState', this.TableState.bind(this));
        proxy.on('UserAuthenticated', this.UserAuthenticated.bind(this));
        proxy.start();
    },
    // Draw Canvas 
    InitCanvas: function(){
        this.stage = new Kinetic.Stage({
            container: 'container',
            width: 800,
            height: 600,
        });
        this.gameLayer = new Kinetic.Layer();
        this.gameLayer.clear();
    },

    DrawPlanets: function (remotePlanets) {
        this.InitCanvas();
        remotePlanets.forEach(function (planet) {
            var circle = new Kinetic.Circle({
                x: planet.X,
                y: planet.Y,
                radius: planet.Radius,
                fill: Game.colors[planet.GroupID],
            });
            var text = new Kinetic.Text({
                x: planet.X,
                y: planet.Y,
                text: planet.ShipCount.toString(),
                fontSize: 14,
                fontFamily: 'Calibri',
                fill: 'black'
            });
            circle.ShipCount = planet.ShipCount;
            circle.Text = text;
            circle.GroupID = planet.GroupID;
            circle.ID = planet.ID;
            circle.on('mousedown', function () {
                if (Game.selectedPlanet == undefined) {
                    if (this.GroupID != Game.currentPlayerGroupID) {
                        return;
                    }
                    Game.selectedPlanet = this;
                    this.setStroke('red');
                } else {
                    // TODO gamoidzaxe Move();
                    proxy.send('move', Game.selectedPlanet.ID, this.ID, Math.ceil(Game.selectedPlanet.ShipCount / 2));
                }
            });
            Game.planets.push(circle);
            Game.gameLayer.add(circle);
            Game.gameLayer.add(text);
        });
        Game.stage.add(Game.gameLayer);
        Game.stage.draw();
    },
    // Server Callbacks ----------------------------------------
    Online: function () {
        console.log('server is online');
    },

    Offline: function () {
        console.log('server is offline');
    },

    PlayerMove : function(userid, fromObject, toObject, shipsCount, animationDuration){

    },

    TableState : function(table){
        switch (table.Status) {
            case 0:
                console.log('joined');
                $('#Notification > .item').hide();
                $('#Notification > .item.waiting_opponent').show();
                jok.setPlayer(1, jok.currentUserID);
                break;
            case 1:
                var opponent = (table.players[0].UserID == jok.currentUserID)? table.players[1].UserID : table.players[0].UserID;
                jok.setPlayer(1, jok.currentUserID);
                jok.setPlayer(2, opponent);
                $('#Notification > .item').hide();
                this.DrawPlanets(table.Planets);
                break;
        }
    },

    UserAuthenticated: function (userid) {
        jok.currentUserID = userid;
    }
}


Game.Init();
