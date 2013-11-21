
var Game = {
    stage: undefined,
    gameLayer: undefined,
    planets: [],
    Game: this,
    currentPlayerGroupID: undefined,
    colors: ['gray', 'green', 'yellow'],
    selectedPlanet: undefined,
    proxy : undefined,

    Init: function () {
        Game.proxy = new GameHub('GameHub', jok.config.sid, jok.config.channel);
        Game.proxy.on('Online', this.Online.bind(this));
        Game.proxy.on('Offline', this.Offline.bind(this));
        Game.proxy.on('PlayerMove', this.PlayerMove.bind(this));
        Game.proxy.on('TableState', this.TableState.bind(this));
        Game.proxy.on('UpdatePlanetsState', this.UpdatePlanetsState.bind(this));
        Game.proxy.on('UserAuthenticated', this.UserAuthenticated.bind(this));
        Game.proxy.start();
    },


    // Server Callbacks ----------------------------------------
    Online: function () {
        console.log('server is online');
    },

    Offline: function () {
        console.log('server is offline');
    },

    PlayerMove: function (userid, fromObject, toObject, shipsCount, animationDuration) {

    },
    
    TableState: function (table) {
        
        table.players.forEach(function (player) {
            console.log(player.GroupID);
            if (player.UserID == jok.currentUserID) {
                Game.currentPlayerGroupID = player.GroupID;
            }
        });
        switch (table.Status) {
            case 0:
                console.log('joined');
                $('#Notification > .item').hide();
                $('#Notification > .item.waiting_opponent').show();
                jok.setPlayer(1, jok.currentUserID);
                break;
            case 1:
                var opponent = (table.players[0].UserID == jok.currentUserID) ? table.players[1].UserID : table.players[0].UserID;
                jok.setPlayer(1, jok.currentUserID);
                jok.setPlayer(2, opponent);
                $('#Notification > .item').hide();
                this.DrawPlanets(table.Planets);
                break;
        }
    },

    UserAuthenticated: function (userid) {
        jok.currentUserID = userid;
    },


    // Draw Canvas 
    InitCanvas: function () {
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
                console.log(this.ID);
                console.log(this.GroupID);
                console.log(Game.currentPlayerGroupID);
                if (Game.selectedPlanet == undefined) {
                    if (this.GroupID != Game.currentPlayerGroupID) {
                        return;
                    }
                    Game.selectedPlanet = this;
                    this.setStroke('red');
                } else {
                    // TODO gamoidzaxe Move();
                    Game.proxy.send('move', Game.selectedPlanet.ID, this.ID, Math.ceil(Game.selectedPlanet.ShipCount / 2));
                }
            });
            Game.planets.push(circle);
            Game.gameLayer.add(circle);
            Game.gameLayer.add(text);
        });
        Game.stage.add(Game.gameLayer);
        Game.stage.draw();
    },


    UpdatePlanetsState: function (remotePlanets) {
        console.log(remotePlanets);
        Game.planets.forEach(function (planet) {
            for (var i = 0; i < remotePlanets.length; i++) {
                if (remotePlanets[i].ID == planet.ID) {
                    planet.ShipCount = remotePlanets[i].ShipCount;
                    planet.GroupID= remotePlanets[i].GroupID;
                    planet.Text.setText(planet.ShipCount.toString());
                    planet.setFill(Game.colors[planet.GroupID]);
                }
            }
            Game.gameLayer.draw();
        });
    }
}


Game.Init();
