using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace YanivDesktop
{
    public enum CardDrawing { DECK, LEFT, RIGHT, NONE }
    public enum CardState { LIFT, PUT_DOWN, HOVER_UP, HOVER_DOWN, NONE }
    [Serializable]
    public class ScoresTable
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public string Date { get; set; }
    }
    public class Yaniv : Game
    {
        // SPRITES
        private SpriteBatch spriteBatch;
        private SpriteFont scoresFont, scoresSignFont, nameFont, tableFont, dateFont;

        private Sprite deckSprite,
            callIt,
            tookCard,
            startButton,
            scoresTableButton,
            backButton,
            nameSignSprite,
            leftPlayerTurnBoldSprite,
            rightPlayerTurnBoldSprite;

        // TEXTURES
        private Texture2D
            assafCallingTexture,
            yanivCallingTexture,
            cantTakeCard,
            cardTexture,
            cardBackTexture,
            gameScreen,
            startupScreen,
            scoresTableScreen,
            startButtonGlowTexture,
            scoresTableGlowTexture,
            backButtonGlowTexture;

        // LISTS
        private List<Player> players;
        private List<Card> tableCards;
        private List<ScoresTable> scoresTable;
        private List<Vector2>[] playersCardsVectors;

        // VECTORS
        private Vector2[] playersCallingSignVectors;
        private Vector2[] playersScoresSignVectors;
        private Vector2 scoresTableVector;
        private Vector2 rotationVector;
        private Vector2 defaultTookCardVector;
        private Vector2 nameVector;

        // BOOLEANS
        private bool openingCardShufflePlayed,
            nameSign,
            startButtonGlow,
            scoresTableButtonGlow,
            backButtonGlow,
            assaf;

        // INTEGERS
        private int[] deck, lastDeletedIndex;
        private int roundNumber,
            screenNumber,
            randomIndex,
            winner,
            caller,
            turnCounter;

        // OTHERS
        private Card deckCard;
        private Random random;
        private CardDrawing[] playersCardDrawings;
        private float currentTime, rotation;
        private MouseState mouseCurrent, mousePrevious;
        private KeyboardState keyboardStateCurrent, keyboardStatePrevious;
        private SoundEffect cardShuffle, cardBeingThrown;
        const float Offset = MathHelper.PiOver4 * 1.5f;
        private StringBuilder nameString;
        GraphicsDeviceManager graphics;

        // --- INITIALIZATION --- //
        public Yaniv()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";                      
            IsMouseVisible = true;            

            // LISTS
            tableCards = new List<Card>();
            players = new List<Player>();
            playersCardDrawings = new CardDrawing[3];
            for (var i = 0; i < 3; i++)
            {
                players.Add(new Player(i, String.Empty));
                playersCardDrawings[i] = CardDrawing.NONE;
            }
            scoresTable = Deserialized();
            scoresTable.Sort((score, other) => score.Score - other.Score);

            // VECTORS
            rotationVector = new Vector2((float)79 / 2, (float)123 / 2);
            defaultTookCardVector = new Vector2(310, 60);
            nameVector = new Vector2(265, 355);
            scoresTableVector = new Vector2(60, 260);
            playersCardsVectors = new[] {
                new List<Vector2>(),
                new List<Vector2>(),
                new List<Vector2>()
            };
            playersScoresSignVectors = new[] {
                new Vector2(450, 390),
                new Vector2(50, 80),
                new Vector2(600, 80)
            };
            playersCallingSignVectors = new[] {
                new Vector2(130, 340),
                new Vector2(110, 60),
                new Vector2(340, 60)
            };

            // OTHERS
            deck = new int[54];
            lastDeletedIndex = new int[3];
            random = new Random();
            nameString = new StringBuilder(string.Empty);
        }

        protected override void Initialize()
        {
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth =
                graphics.PreferredBackBufferHeight = 700;
            Window.Title = "Yaniv Card Game";
            graphics.ApplyChanges();
            base.Initialize();
            screenNumber = 0;
            SetGameSettings();
        }

        private void SetGameSettings()
        {
            // SET STARTING VALUES 
            if (roundNumber == 0) roundNumber = 1;
            winner = 4;
            currentTime = 0f;
            openingCardShufflePlayed = false;

            // CARDS & PLAYERS
            randomIndex = 53;
            for (var i = 0; i < 54; i++) deck[i] = i;
            // Set first table card
            tableCards.Clear();
            var tableCard = GenerateDeckCard();
            tableCard.spriteVector = new Vector2(310, 220);
            tableCards.Add(tableCard);
            // Clean old cards for players and deal
            for (var i = 0; i < players.Count; i++)
            {
                playersCardsVectors[i].Clear();
                players[i].ResetPlayer();
                Deal(players[i]);
                if (roundNumber == 1)
                    players[i].Score = 0;
            }

            for (var k = 0; k < 7; k++)
            {
                playersCardsVectors[0].Add(players[0].Cards[k].spriteVector = new Vector2(55 + k * 85, 500));
                playersCardsVectors[1].Add(new Vector2(50 - k * k + 6f * k, 155 + 20f * k));
                playersCardsVectors[2].Add(new Vector2(570 + k * k - 6f * k, 155 + 20f * k));
            }
        }

        // --- GRAPHIC METHODS --- //
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // --- STARTUP SCREEN ---
            startupScreen = Content.Load<Texture2D>("startupScreen");
            nameFont = Content.Load<SpriteFont>("nameFont");
            tableFont = Content.Load<SpriteFont>("tableFont");
            dateFont = Content.Load<SpriteFont>("dateFont");
            startButton = new Sprite(Content.Load<Texture2D>("startButton"), new Vector2(200, 450));
            startButtonGlowTexture = Content.Load<Texture2D>("startButtonGlowing");
            scoresTableButton = new Sprite(Content.Load<Texture2D>("scoresTableSign"),
                new Vector2(100, 550));
            scoresTableGlowTexture = Content.Load<Texture2D>("scoresTableSignGlow");
            nameSignSprite = new Sprite(Content.Load<Texture2D>("enterNameSign"), new Vector2(41, 412));

            // --- SCORES TABLE ---
            scoresTableScreen = Content.Load<Texture2D>("scoresTableScreen");
            backButton = new Sprite(Content.Load<Texture2D>("backButton"), new Vector2(240, 550));
            backButtonGlowTexture = Content.Load<Texture2D>("backButtonGlow");

            // --- GAME SPRITES, TEXTURES AND SOUND ---
            gameScreen = Content.Load<Texture2D>("gameScreen");

            // Cards
            deckSprite = new Sprite(Content.Load<Texture2D>("deck"), new Vector2(290, 30));
            deckCard = new Card(Shapes.CLUBS, 5, Content.Load<Texture2D>("card"));
            cantTakeCard = Content.Load<Texture2D>("cantTakeCard");
            cardShuffle = Content.Load<SoundEffect>("CardsShuffle");
            cardBeingThrown = Content.Load<SoundEffect>("CardThrown");
            cardTexture = Content.Load<Texture2D>("card");
            cardBackTexture = Content.Load<Texture2D>("backOfCard");
            tookCard = new Sprite(cardBackTexture, defaultTookCardVector, cardBackTexture.Bounds);

            // Signs
            scoresFont = Content.Load<SpriteFont>("scoresFont");
            scoresSignFont = Content.Load<SpriteFont>("scoresSignFont");
            leftPlayerTurnBoldSprite = new Sprite(Content.Load<Texture2D>("leftPlayerTurn"), new Vector2(123, 130));
            rightPlayerTurnBoldSprite = new Sprite(Content.Load<Texture2D>("rightPlayerTurn"), new Vector2(533, 130));
            yanivCallingTexture = Content.Load<Texture2D>("yanivCalling");
            assafCallingTexture = Content.Load<Texture2D>("assafCalling");
            callIt = new Sprite(Content.Load<Texture2D>("callIt"), new Vector2(154, 415));
        }

        protected override void UnloadContent() { }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            switch (screenNumber % 3)
            {
                case 1:
                    {
                        // Game screen
                        spriteBatch.Draw(gameScreen, Vector2.Zero, null, Color.White);
                        // Highlighting a player's title on his turn
                        switch (turnCounter % 3)
                        {
                            case 1:
                                spriteBatch.Draw(leftPlayerTurnBoldSprite.SpriteTexture,
                                    leftPlayerTurnBoldSprite.spriteVector,
                                    leftPlayerTurnBoldSprite.spriteRectangle, Color.White);
                                break;
                            case 2:
                                spriteBatch.Draw(rightPlayerTurnBoldSprite.SpriteTexture,
                                    rightPlayerTurnBoldSprite.spriteVector,
                                    rightPlayerTurnBoldSprite.spriteRectangle, Color.White);
                                break;
                        }

                        // Signs
                        // Scores
                        foreach (var vector in playersScoresSignVectors)
                        {
                            spriteBatch.DrawString(scoresSignFont, "Score",
                                vector, Color.Black);
                        }

                        foreach (var player in players)
                        {
                            spriteBatch.DrawString(scoresFont, player.Score.ToString(),
                                playersScoresSignVectors[player.PlayerNumber] +
                                Vector2.UnitY * 35 + Vector2.UnitX * 25, Color.White);
                        }

                        //call it
                        if (players[0].CardSum() <= 7 && winner == 4 && turnCounter % 3 == 0)
                            spriteBatch.Draw(callIt.SpriteTexture, callIt.spriteVector, Color.White);

                        // --- CARDS --- //
                        // deck
                        spriteBatch.Draw(deckSprite.SpriteTexture, deckSprite.spriteVector, deckSprite.spriteRectangle,
                            Color.White);
                        // table cards
                        for (var i = 0; i < tableCards.Count; i++)
                        {
                            spriteBatch.Draw(tableCards[i].SpriteTexture, tableCards[i].spriteVector,
                                tableCards[i].spriteRectangle, Color.White);
                            if (tableCards.Count > 2 && i != 0 && i != tableCards.Count - 1)
                                spriteBatch.Draw(cantTakeCard, tableCards[i].spriteVector, null, Color.White);
                        }

                        if (winner == 4)
                        {
                            // Players' cards 
                            for (var j = 0; j < 3; j++)
                            {
                                var direction = j == 2 ? -1 : j;
                                var gameCardsTexture = j == 0 ? cardTexture : cardBackTexture;
                                var middle = playersCardsVectors[j].Count % 2 == 0
                                    ? playersCardsVectors[j].Count / 2
                                    : (playersCardsVectors[j].Count - 1) / 2;
                                var tookCardRectangle = deckCard.spriteRectangle;
                                var tookCardTexture = deckCard.SpriteTexture;
                                for (var i = 0; i < players[j].Cards.Count; i++)
                                {
                                    var gameCardsRectangle =
                                        j == 0 ? players[j].Cards[i].spriteRectangle : tookCard.spriteRectangle;
                                    if (i == middle && playersCardDrawings[j] != CardDrawing.NONE)
                                    {
                                        // Drawing the pulled card moving on the table 
                                        spriteBatch.Draw(tookCardTexture,
                                            rotationVector + tookCard.spriteVector,
                                            tookCardRectangle, Color.White,
                                            rotation,
                                            rotationVector, 1, SpriteEffects.None, 1f);
                                    }
                                    else
                                    {
                                        var vector = j == 0 ? players[0].Cards[i].spriteVector : playersCardsVectors[j][i];
                                        spriteBatch.Draw(gameCardsTexture,
                                            rotationVector + vector,
                                            gameCardsRectangle, Color.White,
                                            (i * (direction * MathHelper.PiOver2 - direction * Offset) / middle) +
                                            direction * Offset,
                                            rotationVector, 1, SpriteEffects.None, 1f);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var card in players[0].Cards)
                                spriteBatch.Draw(card.SpriteTexture, card.spriteVector, card.spriteRectangle, Color.White);
                            // Yaniv sign
                            spriteBatch.Draw(yanivCallingTexture, playersCallingSignVectors[caller], Color.White);

                            // Draw caller's cards
                            if (currentTime <= 3f)
                            {
                                for (var i = 1; i < 3; i++)
                                {
                                    var direction = i == 1 ? 1 : -1;
                                    var middle = playersCardsVectors[i].Count % 2 == 0
                                        ? playersCardsVectors[i].Count / 2
                                        : (playersCardsVectors[i].Count - 1) / 2;
                                    var texture = caller == i ? cardTexture : cardBackTexture;
                                    for (var j = 0; j < players[i].Cards.Count; j++)
                                    {
                                        var index = caller == 2 ? players[i].Cards.Count - 1 - j : j;
                                        var rectangle = caller == i
                                            ? players[i].Cards[index].spriteRectangle
                                            : cardBackTexture.Bounds;
                                        spriteBatch.Draw(texture,
                                            rotationVector + playersCardsVectors[i][index],
                                            rectangle, Color.White,
                                            direction * ((index * (MathHelper.PiOver2 - Offset) / middle) +
                                                         Offset),
                                            rotationVector, 1, SpriteEffects.None, 1f);
                                    }
                                }
                            }

                            if (currentTime > 3f)
                            {
                                if (winner != caller)
                                    spriteBatch.Draw(assafCallingTexture, playersCallingSignVectors[winner], null,
                                        Color.White);
                                for (var i = 1; i < 3; i++)
                                {
                                    var direction = i == 2 ? -1 : i;
                                    var middle = playersCardsVectors[i].Count % 2 == 0
                                        ? playersCardsVectors[i].Count / 2
                                        : (playersCardsVectors[i].Count - 1) / 2;
                                    for (var j = 0; j < players[i].Cards.Count; j++)
                                    {
                                        var index = i == 2 ? players[i].Cards.Count - 1 - j : j;
                                        spriteBatch.Draw(cardTexture,
                                            rotationVector + playersCardsVectors[i][index],
                                            players[i].Cards[index].spriteRectangle, Color.White,
                                            direction * ((index * (MathHelper.PiOver2 - Offset) / middle) +
                                                         Offset),
                                            rotationVector, 1, SpriteEffects.None, 1f);
                                    }
                                }
                            }

                            if (currentTime > 5f)
                            {
                                roundNumber--;
                                turnCounter = winner;
                                if (roundNumber <= 0)
                                {
                                    AddScoreToTable();
                                    screenNumber = 2;
                                }

                                SetGameSettings();
                                spriteBatch.End();
                                base.Draw(gameTime);
                                return;
                            }
                        }

                        break;
                    }
                case 0:
                    {
                        // Displaying setting for game gameScreen
                        spriteBatch.Draw(startupScreen, Vector2.Zero, null, Color.White);
                        spriteBatch.Draw(startButtonGlow ? startButtonGlowTexture : startButton.SpriteTexture,
                            startButton.spriteVector, null, Color.White);
                        if (nameSign)
                            spriteBatch.Draw(nameSignSprite.SpriteTexture, nameSignSprite.spriteVector, null, Color.White);
                        spriteBatch.Draw(scoresTableButtonGlow ? scoresTableGlowTexture : scoresTableButton.SpriteTexture,
                            scoresTableButton.spriteVector, null, Color.White);
                        /*foreach (var letter in typedName) {
                            spriteBatch.Draw(letter.SpriteTexture, letter.spriteVector, letter.spriteRectangle,
                                Color.White);
                        }*/
                        spriteBatch.DrawString(nameFont, nameString, nameVector, Color.Black);

                        break;
                    }
                default:
                    {
                        spriteBatch.Draw(scoresTableScreen, Vector2.Zero, null, Color.White);
                        spriteBatch.Draw(backButtonGlow ? backButtonGlowTexture : backButton.SpriteTexture,
                            backButton.spriteVector, null, Color.White);
                        // Print names and scores in table
                        for (var i = 0; i < scoresTable.Count; i++)
                        {
                            var nameAndScore = scoresTable[i];
                            spriteBatch.DrawString(tableFont, nameAndScore.Name,
                                scoresTableVector + Vector2.UnitY * i * 65, Color.Black);
                            spriteBatch.DrawString(tableFont, nameAndScore.Score.ToString(),
                                scoresTableVector + Vector2.UnitX * 325 + Vector2.UnitY * i * 65,
                                Color.Black);
                            spriteBatch.DrawString(dateFont, nameAndScore.Date,
                                scoresTableVector + Vector2.UnitX * 420 + Vector2.UnitY * 5 + Vector2.UnitY * i * 65,
                                Color.Black);
                        }

                        break;

                    }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void UpdateTookCardSprite()
        {
            for (var i = 0; i < 3; i++)
            {
                var middle = players[i].Cards.Count % 2 == 0
                    ? players[i].Cards.Count / 2
                    : (players[i].Cards.Count - 1) / 2;
                var vector = i == 0 ? players[0].Cards[middle].spriteVector : playersCardsVectors[i][middle];
                var direction = i == 2 ? -1 : i;
                if (playersCardDrawings[i] == CardDrawing.NONE) continue;
                // Setting up the starter point for tookCard
                if (tookCard.spriteVector.Equals(defaultTookCardVector))
                {
                    if (playersCardDrawings[i] != CardDrawing.DECK)
                    {
                        var index = playersCardDrawings[i] == CardDrawing.LEFT ? 0 : tableCards.Count - 1;
                        tookCard.spriteVector = tableCards[index].spriteVector;
                    }

                    // Fixing the middle for player 0
                    if (i == 0)
                        tookCard.spriteVector.X = vector.X;
                }

                if (i == 0)
                {
                    if (vector.Y - tookCard.spriteVector.Y > 0.5f)
                    {
                        tookCard.spriteVector.Y += 0.1f * (vector.Y - tookCard.spriteVector.Y);
                        return;
                    }

                    tookCard.spriteVector.Y = vector.Y;
                }
                else
                {
                    if (tookCard.spriteVector.Y < vector.Y)
                    {
                        tookCard.spriteVector.Y += 10;
                        return;
                    }

                    if (tookCard.spriteVector.Y > vector.Y)
                        tookCard.spriteVector.Y = vector.Y;

                    if (direction * rotation < MathHelper.PiOver2)
                    {
                        rotation += direction * 0.5f;
                        return;
                    }

                    if (direction * rotation > MathHelper.PiOver2)
                        rotation = direction * MathHelper.PiOver2;

                    if (direction * (tookCard.spriteVector.X - vector.X) > 0.5f)
                    {
                        tookCard.spriteVector.X += 0.1f * (vector.X - tookCard.spriteVector.X);
                        return;
                    }

                    tookCard.spriteVector.X = vector.X;
                }

                if (tookCard.spriteVector.Equals(vector))
                {
                    playersCardDrawings[i] = CardDrawing.NONE;
                    tookCard.spriteVector = defaultTookCardVector;
                    players[0].Cards.Sort();
                    UpdatePlayersCardsVectors(0);
                    rotation = 0;
                }
            }

        }

        // --- GAME FLOW --- //
        protected override void Update(GameTime gameTime)
        {
            currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            mousePrevious = mouseCurrent;
            mouseCurrent = Mouse.GetState();
            keyboardStatePrevious = keyboardStateCurrent;
            keyboardStateCurrent = Keyboard.GetState();
            switch (screenNumber % 3)
            {
                case 1:
                    {
                        // Play the card shuffle sound at the start of the game
                        if (!openingCardShufflePlayed)
                        {
                            cardShuffle.Play();
                            openingCardShufflePlayed = true;
                        }

                        // If there is a winner for this game, Start a new one or close game
                        if (winner < 4)
                        {
                            base.Update(gameTime);
                            return;
                        }

                        // Mouse hover animation vectors update
                        foreach (var card in players[0].Cards)
                        {
                            if (card.MouseHovered(mouseCurrent) && card.CardState != CardState.LIFT)
                            {
                                card.CardState = CardState.HOVER_UP;
                            }
                            else if (mouseCurrent != mousePrevious && card.CardState == CardState.HOVER_UP)
                                card.CardState = CardState.HOVER_DOWN;

                            switch (card.CardState)
                            {
                                case CardState.LIFT:
                                    if (card.spriteVector.Y > 450)
                                        card.spriteVector.Y -= 10;
                                    break;
                                case CardState.PUT_DOWN:
                                    if (card.spriteVector.Y < 500)
                                        card.spriteVector.Y += 10;
                                    else card.CardState = CardState.NONE;
                                    break;
                                case CardState.HOVER_UP:
                                    if (card.spriteVector.Y > 490)
                                        card.spriteVector.Y -= 5;
                                    break;
                                case CardState.HOVER_DOWN:
                                    if (card.spriteVector.Y < 500)
                                        card.spriteVector.Y += 5;
                                    else card.CardState = CardState.NONE;
                                    break;

                            }
                        }

                        // Player's turn
                        if (turnCounter % 3 == 0)
                        {

                            // Display call it button
                            if (players[0].CardSum() <= 7 &&
                                callIt.MouseTouched(mouseCurrent, mousePrevious))
                            {
                                CheckWhoIsTheWinner(players[0]);
                                base.Update(gameTime);
                                return;
                            }

                            // Clicking and picking cards to throw
                            foreach (var c in players[0].Cards.Where(c => c.MouseTouched(mouseCurrent, mousePrevious)))
                            {
                                players[0].PickCard(c);
                            }

                            // Clicking on one of the cards on the table
                            if (tableCards[0].MouseTouched(mouseCurrent, mousePrevious) ||
                                tableCards[tableCards.Count - 1].MouseTouched(mouseCurrent, mousePrevious) ||
                                deckSprite.MouseTouched(mouseCurrent, mousePrevious))
                            {
                                if (!CheckIfLegal(players[0].PickedCards))
                                {
                                    base.Update(gameTime);
                                    return;
                                }

                                var index = tableCards[0].MouseTouched(mouseCurrent, mousePrevious) &&
                                            !tableCards[tableCards.Count - 1].MouseTouched(mouseCurrent, mousePrevious)
                                    ? 0
                                    : tableCards[0].MouseTouched(mouseCurrent, mousePrevious) &&
                                      tableCards[tableCards.Count - 1].MouseTouched(mouseCurrent, mousePrevious)
                                        ? tableCards.Count - 1
                                        : !tableCards[0].MouseTouched(mouseCurrent, mousePrevious) &&
                                          tableCards[tableCards.Count - 1].MouseTouched(mouseCurrent, mousePrevious)
                                            ? tableCards.Count - 1
                                            : -1;

                                var cardToTake = index == -1 ? GenerateDeckCard() : tableCards[index];
                                deckCard = new Card(cardToTake);
                                playersCardDrawings[0] = index switch
                                {
                                    -1 => CardDrawing.DECK,
                                    0 => CardDrawing.LEFT,
                                    _ => CardDrawing.RIGHT
                                };
                                cardBeingThrown.CreateInstance().Play();
                                ReplaceTableCards(players[0].Play(new Card(cardToTake)));
                                // Switch last card with middle card
                                var middle = players[0].Cards.Count % 2 == 0
                                    ? players[0].Cards.Count / 2
                                    : (players[0].Cards.Count - 1) / 2;
                                var middleCard = players[0].Cards[middle];
                                players[0].Cards[middle] = players[0].Cards[players[0].Cards.Count - 1];
                                players[0].Cards[players[0].Cards.Count - 1] = middleCard;
                                UpdatePlayersCardsVectors(0);
                                turnCounter++;
                                currentTime = 0;
                            }

                            // Computer's turn
                        }
                        else
                        {
                            if (currentTime > 3.5f)
                            {
                                ComputerStrategy(players[turnCounter % 3]);
                                turnCounter++;
                                currentTime = 0f;
                            }
                        }

                        UpdateTookCardSprite();
                        break;
                    }
                case 0:
                    {
                        startButtonGlow = startButton.MouseHovered(mouseCurrent);
                        scoresTableButtonGlow = scoresTableButton.MouseHovered(mouseCurrent);
                        if (startButton.MouseTouched(mouseCurrent, mousePrevious))
                        {
                            if (nameString.Length == 0) nameSign = true;
                            else
                            {
                                screenNumber = 1;
                                SetGameSettings();
                            }
                        }
                        if (scoresTableButton.MouseTouched(mouseCurrent, mousePrevious)) screenNumber = 2;
                        var keys = keyboardStatePrevious.GetPressedKeys();
                        var shift = keys.Any(key => key == Keys.LeftShift || key == Keys.RightShift);
                        foreach (var key in keys)
                        {
                            var x = (char)key;
                            if (!keyboardStatePrevious.IsKeyDown(key) || !keyboardStateCurrent.IsKeyUp(key)) continue;
                            if (key == Keys.Back && nameString.Length > 0)
                                nameString.Remove(nameString.Length - 1, 1);
                            else
                            {
                                if (65 <= x && x <= 90 && nameString.Length < 11)
                                    nameString.Append(shift ? x : (char)(x + 32));
                            }

                            players[0].PlayerName = nameString.ToString();
                        }

                        break;
                    }
                default:
                    if (backButton.MouseTouched(mouseCurrent, mousePrevious)) screenNumber = 0;
                    backButtonGlow = backButton.MouseHovered(mouseCurrent);
                    break;
            }

            base.Update(gameTime);
        }

        private Card GenerateDeckCard()
        {
            if (randomIndex <= 0) randomIndex = 53;
            var index = random.Next(0, randomIndex);
            var shape = deck[index] / 13;
            var value = deck[index] % 13;
            deck[index] = deck[randomIndex];
            var card = new Card((Shapes)shape, value, Content.Load<Texture2D>("card"));
            randomIndex--;
            return card;
        }

        private void AddScoreToTable()
        {
            var newScore = new ScoresTable()
            {
                Name = players[0].PlayerName,
                Score = players[0].Score,
                Date = DateTime.Today.ToShortDateString()
            };
            if (scoresTable.Count < 4)
                scoresTable.Add(newScore);
            else if (newScore.Score < scoresTable[scoresTable.Count - 1].Score)
                scoresTable[scoresTable.Count - 1] = newScore;
            scoresTable.Sort(((score, other) => score.Score - other.Score));
        }

        private void UpdatePlayersCardsVectors(int i)
        {
            if (players[i].Cards.Count < playersCardsVectors[i].Count)
            {
                var oldCount = playersCardsVectors[i].Count;
                var index = lastDeletedIndex[i];
                for (var j = 0; j < oldCount - players[i].Cards.Count; j++)
                {
                    index = index == 0 ? playersCardsVectors[i].Count - 1 : 0;
                    playersCardsVectors[i].RemoveAt(index);
                }

                lastDeletedIndex[i] = index;
            }

            if (i == 0)
                for (var j = 0; j < players[0].Cards.Count; j++)
                {
                    var addition = players[0].Cards.Count % 2 == 0 ? 45 : 0;
                    players[0].Cards[j].spriteVector = playersCardsVectors[0][j] + Vector2.UnitX * addition;
                }

        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            Serialize(scoresTable);
            base.OnExiting(sender, args);
        }

        private void Serialize(List<ScoresTable> scoresTableList)
        {
            IFormatter formatter = new BinaryFormatter();
            try
            {
                Stream stream = new FileStream("ScoresTable.bin", FileMode.OpenOrCreate, FileAccess.Write,
                    FileShare.None);
                formatter.Serialize(stream, scoresTableList);
                stream.Close();
            }
            catch (Exception)
            {

            }
        }

        private List<ScoresTable> Deserialized()
        {
            var list = new List<List<ScoresTable>>();
            IFormatter formatter = new BinaryFormatter();
            try
            {
                Stream stream = new FileStream("ScoresTable.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
                while (stream.Position < stream.Length)
                {
                    var obj = (List<ScoresTable>)formatter.Deserialize(stream);
                    list.Add(obj);
                }

                stream.Close();
            }
            catch (Exception)
            {
            }

            return list.Count > 0 ? list[0] : new List<ScoresTable>();
        }

        private bool CheckIfLegal(List<Card> cards)
        {
            if (cards.Count == 0) return false;
            if (cards.Count == 1) return true;

            bool series;
            bool sameValue;

            series = CheckSeries(cards).Count >= 3;
            var value = cards[0].CardValue;
            sameValue = cards.TrueForAll(card => card.CardValue == value);

            return sameValue ^ series;
        }

        private void ReplaceTableCards(List<Card> cards)
        {
            if (cards.Count == 0) return;

            cards.Sort();
            var jokers = cards.FindAll(card => card.CardShape == Shapes.JOKER);
            foreach (var joker in jokers)
                joker.CardValue = joker.spriteRectangle.X / 79;

            var middle = cards.Count % 2 == 0 ? cards.Count / 2 : (cards.Count - 1) / 2;
            cards[middle].spriteVector.X = 310;
            cards[middle].spriteVector.Y = 220;

            for (var i = 1; i <= middle; i++)
            {
                cards[middle - i].spriteVector =
                    cards[middle].spriteVector - Vector2.UnitX * 30 * i;
                if (middle + i >= cards.Count) continue;
                cards[middle + i].spriteVector =
                    cards[middle].spriteVector + Vector2.UnitX * 30 * i;
            }

            tableCards.Clear();
            tableCards.AddRange(cards);
        }

        private void ComputerStrategy(Player player)
        {

            if (player.CardSum() <= 7)
            {
                CheckWhoIsTheWinner(player);
                return;
            }

            Card cardToTake = null;
            var seriesWithoutTableCardSum = 0;
            var seriesWithTableCardSum = 0;

            var doNotThrow = new List<Card>();
            var doThrow = new List<Card>();
            var optionalThrow = new List<Card>();

            player.Cards.Sort();
            player.Cards.Reverse();

            //    Throwing cards without relation to the table cards
            //---------------------------------------------------------

            // Check if we can throw a series
            foreach (var shapeCards in
                player.OrganizedCards.Where(shapeCard => shapeCard.Count > 1))
            {
                var series = new List<Card>(shapeCards);
                series.AddRange(player.JokerCards);
                series = CheckSeries(series);
                var currentSum = Card.CardSum(series);
                if (currentSum > seriesWithoutTableCardSum) seriesWithoutTableCardSum = currentSum;
                else continue;
                doThrow.Clear();
                doThrow.AddRange(series);
            }

            // Check double cards to throw
            var sameCardValue = -1;
            var sameCardSum = -1;
            for (var i = 0; i < player.Cards.Count - 1; i++)
            {
                if (player.Cards[i].CompareTo(player.Cards[i + 1]) != 0 ||
                    player.Cards[i].CardShape == Shapes.JOKER ||
                    player.Cards[i + 1].CardShape == Shapes.JOKER) continue;

                if (sameCardValue != -1 && sameCardValue != player.Cards[i].CardValue) break;

                if (sameCardValue == -1)
                    sameCardSum = sameCardValue = player.Cards[i].CardValue;


                // If the player can throw a series too, check with cards 
                // he should throw based on the value. 
                sameCardSum += player.Cards[i].CardValue;

                if (seriesWithoutTableCardSum >= sameCardSum)
                    continue;

                if (!optionalThrow.Contains(player.Cards[i]))
                    optionalThrow.Add(player.Cards[i]);
                optionalThrow.Add(player.Cards[i + 1]);
            }

            //    Check if there is a throw on the next round based on the table cards
            //--------------------------------------------------------------------------
            var allowedToTake = new List<Card>() { tableCards[0] };
            if (tableCards.Count > 1)
                allowedToTake.Add(tableCards[tableCards.Count - 1]);

            // Check if one of the table cards complete a series
            for (var i = 0; i < allowedToTake.Count; i++)
            {
                var card = allowedToTake[i];
                if (card.CardShape == Shapes.JOKER)
                {
                    cardToTake = card;
                    playersCardDrawings[player.PlayerNumber] = i == 0 ? CardDrawing.LEFT : CardDrawing.RIGHT;
                    break;
                }

                var shapeCards = card.CardShape switch
                {
                    Shapes.CLUBS => new List<Card>(player.ClubsCards),
                    Shapes.HEARTS => new List<Card>(player.HeartsCards),
                    Shapes.SPADES => new List<Card>(player.SpadesCards),
                    _ => new List<Card>(player.DiamondCards)
                };

                shapeCards.AddRange(player.JokerCards);
                shapeCards.Add(card);
                shapeCards = CheckSeries(shapeCards);
                seriesWithTableCardSum = Card.CardSum(shapeCards);
                if (!shapeCards.Contains(card) || seriesWithTableCardSum <= seriesWithoutTableCardSum) continue;
                // Throwing series next turn with the new card
                cardToTake = card;
                doNotThrow = shapeCards;
                playersCardDrawings[player.PlayerNumber] = i == 0 ? CardDrawing.LEFT : CardDrawing.RIGHT;
            }

            // Check if one of the table cards exists in players cards.
            if (cardToTake == null || cardToTake.CardShape != Shapes.JOKER)
                for (var i = 0; i < allowedToTake.Count; i++)
                {
                    var tableCard = allowedToTake[i];
                    var allAppearances = player.Cards.FindAll(card => card.CompareTo(tableCard) == 0);
                    if (allAppearances.Count * tableCard.CardValue <= seriesWithoutTableCardSum &&
                        (!optionalThrow.Exists(card => card.CompareTo(tableCard) == 0) ||
                         allAppearances.Count * tableCard.CardValue <= seriesWithTableCardSum)) continue;
                    doNotThrow = allAppearances;
                    cardToTake = tableCard;
                    playersCardDrawings[player.PlayerNumber] = i == 0 ? CardDrawing.LEFT : CardDrawing.RIGHT;
                }

            // If the cards on the table don't fit a double throw or a series completion
            // check if one of the table cards has a small value
            if (cardToTake == null)
                for (var i = 0; i < allowedToTake.Count; i++)
                {
                    if (tableCards[i].CardValue > 2) continue;
                    if (cardToTake != null && cardToTake.CardValue <= tableCards[i].CardValue) continue;
                    cardToTake = tableCards[i];
                    playersCardDrawings[player.PlayerNumber] = i == 0 ? CardDrawing.LEFT : CardDrawing.RIGHT;
                }

            // Take from deck
            if (cardToTake == null)
            {
                playersCardDrawings[player.PlayerNumber] = CardDrawing.DECK;
                cardToTake = GenerateDeckCard();
            }

            // Fix options after all the changes.
            foreach (var card in doNotThrow)
            {
                doThrow.Remove(card);
                optionalThrow.Remove(card);
            }

            doThrow = CheckSeries(doThrow);


            // If no cards got picked, pick the biggest card
            var theEmptyOption = doThrow.Count == 0 ? ref doThrow : ref optionalThrow;
            if (theEmptyOption.Count == 0)
            {
                var bCard = player.Cards.Find(card =>
                    !tableCards.Exists(c => c.CompareTo(card) == 0) &&
                    !doNotThrow.Contains(card) && card.CardShape != Shapes.JOKER);
                if (bCard != null) theEmptyOption.Add(bCard);
            }
            // if there are two options, Choose the better one, or if the plans changed.
            if (Card.CardSum(doThrow) < Card.CardSum(optionalThrow))
            {
                doThrow = optionalThrow;
            }

            player.PickCards(doThrow);
            switch (playersCardDrawings[player.PlayerNumber])
            {
                case CardDrawing.DECK:
                    deckCard.SpriteTexture = cardBackTexture;
                    deckCard.spriteRectangle = cardBackTexture.Bounds;
                    break;
                case CardDrawing.LEFT:
                    deckCard = new Card(tableCards[0]);
                    break;
                case CardDrawing.RIGHT:
                    deckCard = new Card(tableCards[tableCards.Count - 1]);
                    break;
                case CardDrawing.NONE:
                    break;
            }

            ReplaceTableCards(player.Play(new Card(cardToTake)));
            UpdatePlayersCardsVectors(player.PlayerNumber);
            cardBeingThrown.CreateInstance().Play();
        }

        private void CheckWhoIsTheWinner(Player player)
        {
            caller = player.PlayerNumber;
            var minSum = int.MaxValue;
            var winnerNumber = 0;
            foreach (var p in players.Where(p => minSum > p.CardSum()))
            {
                minSum = p.CardSum();
                winnerNumber = p.PlayerNumber;
            }

            if (minSum < player.CardSum())
            {
                assaf = true;
            }

            winner = winnerNumber;
            foreach (var p in players)
            {
                p.ScorePlayer(p == player && assaf, p == player);
            }
        }

        private List<Card> CheckSeries(List<Card> cards)
        {
            if (cards.Count == 0) return new List<Card>();
            var series = new List<Card>();
            var jokers = new List<Card>(cards);
            jokers.RemoveAll(c => c.CardShape != Shapes.JOKER);
            var pickedCardsWnj = new List<Card>(cards);
            pickedCardsWnj.RemoveAll(c => c.CardShape == Shapes.JOKER);

            pickedCardsWnj.Sort();
            pickedCardsWnj.Reverse();

            // Check if all of the card has the same shape
            if (pickedCardsWnj.Count > 0 &&
                !pickedCardsWnj.TrueForAll(card => card.CardShape == pickedCardsWnj[0].CardShape)) return series;

            var lastSeriesValue = -1;
            for (var i = 0; i < pickedCardsWnj.Count - 1; i++)
            {
                if (pickedCardsWnj[i].CardValue - 1 == pickedCardsWnj[i + 1].CardValue &&
                    (lastSeriesValue == -1 || lastSeriesValue == pickedCardsWnj[i].CardValue))
                {
                    if (lastSeriesValue != pickedCardsWnj[i].CardValue)
                        series.Add(pickedCardsWnj[i]);
                    series.Add(pickedCardsWnj[i + 1]);
                    lastSeriesValue = pickedCardsWnj[i + 1].CardValue;
                }
                else if (pickedCardsWnj[i].CardValue - jokers.Count - 1 == pickedCardsWnj[i + 1].CardValue
                         && (lastSeriesValue == -1 || lastSeriesValue == pickedCardsWnj[i].CardValue))
                {
                    if (lastSeriesValue != pickedCardsWnj[i].CardValue)
                        series.Add(pickedCardsWnj[i]);
                    lastSeriesValue = pickedCardsWnj[i].CardValue;
                    for (var j = 0;
                        j < pickedCardsWnj[i].CardValue - pickedCardsWnj[i + 1].CardValue && jokers.Count > 0;
                        j++)
                    {
                        var joker = jokers[0];
                        joker.CardValue = --lastSeriesValue;
                        series.Add(joker);
                        jokers.RemoveAt(0);
                    }

                    series.Add(pickedCardsWnj[i + 1]);
                }
            }

            if (series.Count == 2 && jokers.Count > 0)
            {
                var joker = jokers[0];
                joker.CardValue = series[1].CardValue + 1 < 13 ? series[0].CardValue + 1 : series[1].CardValue - 1;
                series.Add(joker);
                jokers.RemoveAt(0);
            }

            return series.Count >= 3 ? series : new List<Card>();
        }

        private void Deal(Player player)
        {
            var playersDeck = new List<Card>();
            for (var i = 0; i < 7; i++)
                playersDeck.Add(GenerateDeckCard());
            playersDeck.Sort();
            player.SetCards(playersDeck);
        }
    }
}