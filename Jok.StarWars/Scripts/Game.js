var Game = {
    stage: undefined,
    gameLayer: undefined,
    planets: [],
    Game: this,
    currentPlayerGroupID: undefined,
    colors: ['gray', 'green', 'yellow'],
    selectedPlanet: undefined,
    proxy : undefined,
    canvasIsDrawn: undefined,
    gameIsOver : undefined,
    Init: function () {
        Game.proxy = new GameHub('GameHub', jok.config.sid, jok.config.channel);
        Game.proxy.on('Online', this.Online.bind(this));
        Game.proxy.on('Offline', this.Offline.bind(this));
        Game.proxy.on('PlayerMove', this.PlayerMove.bind(this));
        Game.proxy.on('TableState', this.TableState.bind(this));
        Game.proxy.on('UpdatePlanetsState', this.UpdatePlanetsState.bind(this));
        Game.proxy.on('UserAuthenticated', this.UserAuthenticated.bind(this));
        Game.proxy.on('Close', this.Close.bind(this));
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
            case 3:
                Game.gameIsOver = true;
                $('#Notification > .item').hide();
                $('#Notification > .item.table_finish_winner > span').html(jok.players[table.LastWinner].nick);
                $('#Notification > .item.table_finish_winner').show();
                break;
        }
    },

    UserAuthenticated: function (userid) {
        jok.currentUserID = userid;
    },

    DrawPlanets: function (remotePlanets) {
        if (Game.canvasIsDrawn != undefined) {
            return;
        }
        Game.canvasIsDrawn = true;
        this.stage = new Kinetic.Stage({
            container: 'container',
            width: 800,
            height: 600,
        });
        this.gameLayer = new Kinetic.Layer();
        this.gameLayer.clear();
        remotePlanets.forEach(function (planet) {
            var circle = new Kinetic.Circle({
                x: planet.X,
                y: planet.Y,
                radius: planet.Radius,
                fill: Game.colors[planet.GroupID],
            });
            circle.Text = new Kinetic.Text({
                text: planet.ShipCount.toString(),
                fontSize: 14,
                fontFamily: 'Calibri',
                fill: 'black'
            });
            circle.Text.setX(circle.getX() - circle.Text.getWidth() / 2);
            circle.Text.setY(circle.getY() - circle.Text.getHeight() / 2);
            circle.Text;
            circle.ShipCount = planet.ShipCount;
            circle.GroupID = planet.GroupID;
            circle.ID = planet.ID;
            circle.RemoveSelection = function () {
                this.setStroke(null);
                Game.selectedPlanet = undefined;
            }
            circle.OnClick = function (e) {
                if (Game.gameIsOver != undefined && Game.gameIsOver) {
                    return;
                }
                if (e.which == 1) {
                    if (Game.selectedPlanet != undefined) {
                        if (Game.selectedPlanet.ID == this.ID) {
                            this.RemoveSelection();
                            return;
                        }
                    }
                    if (Game.selectedPlanet == undefined) {
                        if (this.GroupID != Game.currentPlayerGroupID) {
                            return;
                        }
                        Game.selectedPlanet = this;
                        this.setStroke('red');
                        Game.gameLayer.draw();
                    }
                }
                else if (e.which == 3) {
                    Game.proxy.send('move', Game.selectedPlanet.ID, this.ID, Math.ceil(Game.selectedPlanet.ShipCount / 2));
                    Game.selectedPlanet.setStroke(null);
                    Game.selectedPlanet = undefined;
                }
            };
            circle.on('click tap', function (e) {
                circle.OnClick(e);
            });
            circle.Text.on('click tap', function (e) {
                circle.OnClick(e);
            });
            Game.planets.push(circle);
            Game.gameLayer.add(circle);
            Game.gameLayer.add(circle.Text);
        });
        Game.stage.add(Game.gameLayer);
        Game.stage.draw();
    },


    UpdatePlanetsState: function (remotePlanets) {
        //console.log(remotePlanets);
        Game.planets.forEach(function (planet) {
            for (var i = 0; i < remotePlanets.length; i++) {
                if (remotePlanets[i].ID == planet.ID) {
                    planet.ShipCount = remotePlanets[i].ShipCount;
                    planet.GroupID= remotePlanets[i].GroupID;
                    planet.Text.setText(planet.ShipCount.toString());
                    planet.Text.setX(planet.getX() - planet.Text.getWidth() / 2);
                    planet.Text.setY(planet.getY() - planet.Text.getHeight() / 2);
                    planet.setFill(Game.colors[planet.GroupID]);
                }
            }
            Game.gameLayer.draw();
        });
    },

    Close : function (reason) {

        $('#Notification > .item').hide();
        $('#Notification > .item.quit > span').html(reason);
        $('#Notification > .item.quit').show();
        $('#Game').hide();
        jok.setPlayer(1, null);
        jok.setPlayer(2, null);
    }
}
Game.Init();
