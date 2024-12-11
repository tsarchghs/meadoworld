using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace MeadoworldMono;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    // Camera and player properties
    private Vector2 _cameraPosition;
    private Vector2 _playerPosition;
    private float _cameraZoom = 1.0f;
    private Matrix _transformMatrix;
    
    // Sprites
    private Texture2D _playerSprite;
    private Texture2D _worldMapTexture;
    private float _playerSpeed = 200f; // Pixels per second

    // Add these properties at the top of the class
    private float _minZoom = 0.5f;
    private float _maxZoom = 2.0f;
    private float _zoomSpeed = 0.2f;
    private Rectangle _worldBounds;
    private float _gameTime = 0f; // Time in hours
    private float _timeScale = 24f; // How many game hours pass per real second
    private Color _ambientColor = Color.White;
    private SpriteFont _font;
    private List<Location> _locations;
    private Texture2D _cityIcon;
    private Texture2D _villageIcon;
    private Texture2D _castleIcon;
    private Location _nearestLocation;
    private GameState _currentState = GameState.WorldMap;
    private bool _wasSpacePressed = false;
    private Inventory _playerInventory = new();
    private Item _selectedItem;
    private int _selectedItemIndex = 0;
    private bool _wasKeyPressed = false;
    private Party _playerParty = new();
    private int _selectedTroopIndex = 0;
    private float _lastWagePayment = 0f;
    private const float WAGE_PAYMENT_INTERVAL = 24f; // Pay wages every 24 game hours
    private bool _isViewingParty = false;
    private Battle _currentBattle;
    private Party _enemyParty;
    private bool _battleStarted = false;
    private bool _isGathering = false;
    private float _gatheringProgress = 0f;
    private const float GATHERING_TIME = 3f; // Hours
    private string _gatheringResult = "";
    private float _gatheringResultTimer = 0f;
    private Weather _weather;
    private Season _season = new();
    private const float BASE_MOVEMENT_SPEED = 200f;
    private string _currentHazard = null;
    private float _hazardTimer = 0f;
    private Random _random = new Random();
    private float _encounterTimer = 0f;
    private const float ENCOUNTER_CHECK_INTERVAL = 1f; // Check every hour
    private CaravanManager _caravanManager;
    private Texture2D _caravanTexture;

    // Add these constants
    private const float TIME_SCALE = 24f;
    private const float DESERTION_THRESHOLD = 0.2f;
    
    // Add these fields
    private bool _isHunting;
    private Inventory _currentTrader;
    private Random random = new Random();

    private Texture2D _pixelTexture;

    private float _partyMorale = 100f;
    private const float MORALE_DECAY_RATE = 0.5f; // Per hour
    private const float FOOD_VARIETY_BONUS = 5f;
    private const float VICTORY_MORALE_BONUS = 10f;
    private const float DEFEAT_MORALE_PENALTY = -15f;

    private List<Quest> _activeQuests = new();
    private Dictionary<Location, int> _reputation = new();

    private FactionManager _factionManager;

    // Add missing texture fields
    private Texture2D _playerTexture;
    private Texture2D _locationTexture;
    private Texture2D _enemyTexture;
    private Texture2D _uiTexture;
    private Texture2D _hazardTexture;
    private Texture2D _questTexture;
    private Texture2D _resourceTexture;

    private KeyboardState _previousKeyboardState;

    // Add this field at the top of the class
    private bool _isBuyMode = true;  // true for buy mode, false for sell mode

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // Set window size
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Set up initial window size
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

        // Initialize camera position at center of screen
        _cameraPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2,
                                    _graphics.PreferredBackBufferHeight / 2);
        
        // Set initial zoom
        _cameraZoom = 1.0f;

        // Initialize player and inventory
        var player = new Player();
        _playerInventory = player.Inventory;  // Use the player's inventory instead of creating a new one

        Console.WriteLine("Game initialized");
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Debug loading with try/catch for each texture
        try
        {
            _worldMapTexture = Content.Load<Texture2D>("kenney/world_map");
            Console.WriteLine("World map loaded successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load world map: {e.Message}");
            _worldMapTexture = CreatePlaceholderTexture(GraphicsDevice, 2048, 2048, Color.Green);
        }

        try
        {
            _playerSprite = Content.Load<Texture2D>("kenney/Navigation/nav_select");
            Console.WriteLine("Player sprite loaded successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load player sprite: {e.Message}");
            _playerSprite = CreatePlaceholderTexture(GraphicsDevice, 32, 32, Color.Blue);
        }

        // Create a basic font if the content font is not available
        try
        {
            _font = Content.Load<SpriteFont>("gameFont");
            Console.WriteLine("Font loaded successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load font: {e.Message}");
            throw; // Font is required for UI, so we should fail if it's not available
        }
        
        // Initialize player position at center of screen
        _playerPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2,
                                    _graphics.PreferredBackBufferHeight / 2);
        _cameraPosition = _playerPosition;

        // Set world boundaries based on the world map texture
        _worldBounds = new Rectangle(0, 0, _worldMapTexture.Width, _worldMapTexture.Height);

        // Initialize locations
        InitializeLocations();

        // Only create CaravanManager if we have enough locations
        if (_locations.Count >= 2)
        {
            _caravanManager = new CaravanManager(_locations);
        }
        else
        {
            // Handle the case where there aren't enough locations
            throw new InvalidOperationException("Game requires at least 2 locations to function properly");
        }

        // Initialize Weather with Season
        _weather = new Weather(_season);

        InitializeQuests();
        InitializeReputation();

        _factionManager = new FactionManager();

        try
        {
            _caravanTexture = Content.Load<Texture2D>("kenney/caravan");
            Console.WriteLine("Caravan texture loaded successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load caravan texture: {e.Message}");
            _caravanTexture = CreatePlaceholderTexture(GraphicsDevice, 32, 32, Color.Brown);
        }

        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }
    private void HandleCaravanTradingInput()
    {
        if (_currentTrader == null)
        {
            _currentState = GameState.WorldMap;
            return;
        }

        var keyboardState = Keyboard.GetState();
        var prevKeyboardState = _previousKeyboardState;

        // Switch between buy/sell modes with Tab
        if (keyboardState.IsKeyDown(Keys.Tab) && prevKeyboardState.IsKeyUp(Keys.Tab))
        {
            _isBuyMode = !_isBuyMode;
            _selectedItemIndex = 0;  // Reset selection when switching modes
        }

        // Navigation
        if (keyboardState.IsKeyDown(Keys.Up) && prevKeyboardState.IsKeyUp(Keys.Up))
        {
            _selectedItemIndex--;
            if (_selectedItemIndex < 0)
                _selectedItemIndex = Math.Max(0, (_isBuyMode ? _currentTrader.Items.Count : _playerInventory.Items.Count) - 1);
        }
        if (keyboardState.IsKeyDown(Keys.Down) && prevKeyboardState.IsKeyUp(Keys.Down))
        {
            _selectedItemIndex++;
            if (_selectedItemIndex >= (_isBuyMode ? _currentTrader.Items.Count : _playerInventory.Items.Count))
                _selectedItemIndex = 0;
        }

        // Handle buying/selling with Space
        if (keyboardState.IsKeyDown(Keys.Space) && prevKeyboardState.IsKeyUp(Keys.Space))
        {
            if (_isBuyMode && _currentTrader.Items.Count > 0)
            {
                var item = _currentTrader.Items.ElementAt(_selectedItemIndex).Key;
                int price = (int)item.BasePrice; // Changed from item.Value
                
                if (_playerInventory.Gold >= price)
                {
                    _playerInventory.Gold -= price;
                    _playerInventory.AddItem(item);
                    _currentTrader.RemoveItem(item);
                }
            }
            else if (!_isBuyMode && _playerInventory.Items.Count > 0)
            {
                var item = _playerInventory.Items.ElementAt(_selectedItemIndex).Key;
                int price = (int)(item.BasePrice * 0.8f); // Sell for 80% of base value
                
                _playerInventory.RemoveItem(item);
                _playerInventory.Gold += price;
                _currentTrader.AddItem(item);
            }
        }

        // Exit trading
        if (keyboardState.IsKeyDown(Keys.Escape) && prevKeyboardState.IsKeyUp(Keys.Escape))
        {
            _currentState = GameState.WorldMap;
        }

        _previousKeyboardState = keyboardState;
    }
    protected override void Update(GameTime gameTime)
    {
        try
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Add this line to update the transform matrix
            _transformMatrix = Matrix.CreateTranslation(
                -_cameraPosition.X + (_graphics.PreferredBackBufferWidth / 2),
                -_cameraPosition.Y + (_graphics.PreferredBackBufferHeight / 2),
                0) *
                Matrix.CreateScale(_cameraZoom);

            Console.WriteLine($"Frame update started. Delta time: {deltaTime}");
            
            // Add caravan update here
            _caravanManager?.Update(deltaTime, _weather, _season);
            
            string terrainString = ResourceGathering.GetTerrainType(_playerPosition);
            HandleTerrainEffects(terrainString);

            // Update party methods without parameters
            _playerParty.ConsumeDailyFood();
            _playerParty.PayWages();

            HandleTerrainEffects(terrainString);

            // Fix SomeMethod to return bool
            bool result = _playerParty.GetTotalTroops() > 0;

            // Fix CalculateFoodSupply call - remove parameter
            float foodSupply = _playerParty.CalculateFoodSupply();

            var keyboardState = Keyboard.GetState();
            
            // Add state-specific update logging
            Console.WriteLine($"Current game state: {_currentState}");
            
            switch (_currentState)
            {
                case GameState.WorldMap:
                    UpdateWorldMap(gameTime, keyboardState);
                    break;
                case GameState.LocationMenu:
                    UpdateLocationMenu(gameTime, keyboardState);
                    break;
                case GameState.Trading:
                    UpdateTrading(gameTime, keyboardState);
                    break;
                case GameState.Recruiting:
                    UpdateRecruiting(gameTime, keyboardState);
                    break;
                case GameState.Battle:
                    UpdateBattle(gameTime, keyboardState);
                    break;
                case GameState.CaravanTrading:
                    UpdateCaravanTrading(gameTime, keyboardState);
                    break;
            }

            Console.WriteLine("Frame update completed successfully");
            base.Update(gameTime);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in Update: {e.Message}");
            Console.WriteLine($"Stack trace: {e.StackTrace}");
            // Optionally exit the game on critical errors
            // Exit();
        }
    }

    private void UpdateWorldMap(GameTime gameTime, KeyboardState keyboardState)
    {
        try
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Console.WriteLine("Updating world map...");
            
            if (_isGathering)
            {
                UpdateGathering(gameTime);
                return;
            }

            string currentTerrain = ResourceGathering.GetTerrainType(_playerPosition);
            
            // Handle movement
            Vector2 movement = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.W)) movement.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.S)) movement.Y += 1;
            if (keyboardState.IsKeyDown(Keys.A)) movement.X -= 1;
            if (keyboardState.IsKeyDown(Keys.D)) movement.X += 1;

            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                _playerPosition += movement * BASE_MOVEMENT_SPEED * deltaTime;
                Console.WriteLine($"Player moved to: {_playerPosition}");
            }

            // Update camera
            _cameraPosition = Vector2.Lerp(_cameraPosition, _playerPosition, 0.1f);
            
            UpdateNearestLocation();
            
            Console.WriteLine("World map update completed");

            // Check for caravan interaction
            if (keyboardState.IsKeyDown(Keys.E) && !_wasKeyPressed && _caravanManager != null)
            {
                var nearestCaravan = _caravanManager.GetNearestCaravan(_playerPosition);
                if (nearestCaravan != null && 
                    Vector2.Distance(_playerPosition, nearestCaravan.Position) < 50f && 
                    nearestCaravan.Inventory != null)
                {
                    _currentTrader = nearestCaravan.Inventory;
                    _currentState = GameState.CaravanTrading;
                    _selectedItemIndex = 0;
                    Console.WriteLine("Entering caravan trading mode");
                }
            }

            _wasKeyPressed = keyboardState.IsKeyDown(Keys.E);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in UpdateWorldMap: {e.Message}");
        }
    }

    private void UpdateLocationMenu(GameTime gameTime, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            _currentState = GameState.WorldMap;
        }
        else if (keyboardState.IsKeyDown(Keys.D1))
        {
            _currentState = GameState.Trading;
        }
        else if (keyboardState.IsKeyDown(Keys.D2))
        {
            // Rest for 8 hours
            _gameTime += 8f;
            if (_gameTime >= 24f) _gameTime -= 24f;
            _currentState = GameState.WorldMap;
        }
        else if (keyboardState.IsKeyDown(Keys.D3))
        {
            _currentState = GameState.Recruiting;
            _selectedTroopIndex = 0;
        }
    }

    private void UpdateTrading(GameTime gameTime, KeyboardState keyboardState)
    {
        // Exit trading menu
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            _currentState = GameState.LocationMenu;
            _selectedItemIndex = 0; // Reset selection when exiting
            return;
        }

        // Get the current inventory we're looking at
        var currentInventory = _nearestLocation.Inventory;
        int itemCount = currentInventory.Items.Count;

        if (itemCount == 0) return; // No items to trade

        // Navigate items
        if (keyboardState.IsKeyDown(Keys.Up) && !_wasKeyPressed)
        {
            _selectedItemIndex = Math.Max(0, _selectedItemIndex - 1);
        }
        if (keyboardState.IsKeyDown(Keys.Down) && !_wasKeyPressed)
        {
            _selectedItemIndex = Math.Min(itemCount - 1, _selectedItemIndex);
        }

        // Buy item
        if (keyboardState.IsKeyDown(Keys.B) && !_wasKeyPressed && itemCount > 0)
        {
            var item = currentInventory.Items.ElementAt(_selectedItemIndex).Key;
            int price = _nearestLocation.GetBuyPrice(item);
            
            if (_playerInventory.Gold >= price && currentInventory.Items.ContainsKey(item))
            {
                _playerInventory.Gold -= price;
                _playerInventory.AddItem(item);
                currentInventory.RemoveItem(item);
                
                // Play success sound or show feedback
            }
        }

        // Sell item
        if (keyboardState.IsKeyDown(Keys.S) && !_wasKeyPressed && _playerInventory.Items.Count > 0)
        {
            if (_selectedItemIndex < _playerInventory.Items.Count)
            {
                var item = _playerInventory.Items.ElementAt(_selectedItemIndex).Key;
                int price = _nearestLocation.GetSellPrice(item);
                
                _playerInventory.RemoveItem(item);
                _playerInventory.Gold += price;
                currentInventory.AddItem(item);
                
                // Play success sound or show feedback
            }
        }

        // Update key press tracking
        _wasKeyPressed = keyboardState.IsKeyDown(Keys.Up) || 
                        keyboardState.IsKeyDown(Keys.Down) || 
                        keyboardState.IsKeyDown(Keys.B) || 
                        keyboardState.IsKeyDown(Keys.S);
    }

    private void UpdateRecruiting(GameTime gameTime, KeyboardState keyboardState)
    {
        var availableTroops = TroopDatabase.GetAvailableTroops(_nearestLocation.Type);
        
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            _currentState = GameState.LocationMenu;
            return;
        }

        // Toggle between available troops and current party
        if (keyboardState.IsKeyDown(Keys.Tab) && !_wasKeyPressed)
        {
            _isViewingParty = !_isViewingParty;
            _selectedTroopIndex = 0;
        }

        if (_isViewingParty)
        {
            UpdatePartyView(keyboardState);
        }
        else
        {
            UpdateRecruitmentView(keyboardState, availableTroops);
        }

        _wasKeyPressed = keyboardState.IsKeyDown(Keys.Up) || 
                        keyboardState.IsKeyDown(Keys.Down) || 
                        keyboardState.IsKeyDown(Keys.Space) ||
                        keyboardState.IsKeyDown(Keys.Tab);
    }

    private void UpdatePartyView(KeyboardState keyboardState)
    {
        var troops = _playerParty.Troops.ToList();
        if (troops.Count == 0) return;

        // Navigate troops
        if (keyboardState.IsKeyDown(Keys.Up) && !_wasKeyPressed)
        {
            _selectedTroopIndex = Math.Max(0, _selectedTroopIndex - 1);
        }
        if (keyboardState.IsKeyDown(Keys.Down) && !_wasKeyPressed)
        {
            _selectedTroopIndex = Math.Min(troops.Count - 1, _selectedTroopIndex);
        }

        // Dismiss troop
        if (keyboardState.IsKeyDown(Keys.Space) && !_wasKeyPressed)
        {
            var selectedTroop = troops[_selectedTroopIndex].Key;
            _playerParty.DismissTroop(selectedTroop);
        }
    }

    private void UpdateRecruitmentView(KeyboardState keyboardState, List<Troop> availableTroops)
    {
        // Navigate troops
        if (keyboardState.IsKeyDown(Keys.Up) && !_wasKeyPressed)
        {
            _selectedTroopIndex = Math.Max(0, _selectedTroopIndex - 1);
        }
        if (keyboardState.IsKeyDown(Keys.Down) && !_wasKeyPressed)
        {
            _selectedTroopIndex = Math.Min(availableTroops.Count - 1, _selectedTroopIndex);
        }

        // Recruit troop
        if (keyboardState.IsKeyDown(Keys.Space) && !_wasKeyPressed && availableTroops.Count > 0)
        {
            var selectedTroop = availableTroops[_selectedTroopIndex];
            if (_playerInventory.Gold >= selectedTroop.RecruitmentCost)
            {
                _playerInventory.Gold -= selectedTroop.RecruitmentCost;
                _playerParty.AddTroop(selectedTroop);
            }
        }
    }

    private void UpdateBattle(GameTime gameTime, KeyboardState keyboardState)
    {
        if (!_battleStarted)
        {
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                _battleStarted = true;
            }
            else if (keyboardState.IsKeyDown(Keys.Escape))
            {
                // Try to flee
                if (random.NextDouble() < 0.5f) // 50% chance to flee
                {
                    _currentState = GameState.WorldMap;
                }
            }
            return;
        }

        _currentBattle.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

        if (_currentBattle.IsComplete)
        {
            if (_currentBattle.PlayerWon && _currentBattle.Rewards == null)
            {
                _currentBattle.GenerateRewards();
            }
            
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                if (_currentBattle.PlayerWon)
                {
                    _currentBattle.Rewards.Claim(_playerParty, _playerInventory);
                }
                _currentState = GameState.WorldMap;
            }
        }
    }

    private void UpdateCaravanTrading(GameTime gameTime, KeyboardState keyboardState)
    {
        HandleCaravanTradingInput();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Begin the main sprite batch with proper parameters
        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null,
            null,
            null,
            _transformMatrix
        );
        
        // Draw the world map
        DrawWorldMap();
        
        _spriteBatch.End();

        // Draw UI elements based on game state
        switch (_currentState)
        {
            case GameState.LocationMenu:
                DrawLocationMenu(gameTime);
                break;
            case GameState.Trading:
                DrawTrading(gameTime);
                break;
            case GameState.Recruiting:
                DrawRecruiting(gameTime);
                break;
            case GameState.Battle:
                DrawBattle(gameTime);
                break;
            case GameState.CaravanTrading:
                DrawCaravanTrading(gameTime);
                break;
        }

        base.Draw(gameTime);
    }

    public void DrawCaravanTrading(GameTime gameTime)
    {
        _spriteBatch.Begin();

        if (_currentTrader == null || _font == null || _pixelTexture == null)
        {
            _spriteBatch.End();
            return;
        }

        // Draw semi-transparent dark background
        var backgroundRect = new Rectangle(250, 200, 800, 400);
        DrawRectangle(_spriteBatch, backgroundRect, new Color(0, 0, 0, 200));

        // Draw title
        Vector2 titlePos = new Vector2(backgroundRect.Center.X, 220);
        string titleText = $"Trading with Caravan - {(_isBuyMode ? "Buy Mode" : "Sell Mode")}";
        Vector2 titleSize = _font.MeasureString(titleText);
        _spriteBatch.DrawString(_font, titleText, titlePos - new Vector2(titleSize.X / 2, 0), Color.Gold);

        // Draw controls help text
        Vector2 controlsPos = new Vector2(backgroundRect.Center.X, 250);
        string controlsText = "Controls: ↑↓ Navigate | Space: Buy/Sell | Tab: Switch Mode | ESC: Exit";
        Vector2 controlsSize = _font.MeasureString(controlsText);
        _spriteBatch.DrawString(_font, controlsText, controlsPos - new Vector2(controlsSize.X / 2, 0), Color.White);

        // Draw items based on current mode
        if (_isBuyMode)
        {
            // Draw caravan inventory
            Vector2 merchantTitlePos = new Vector2(300, 280);
            _spriteBatch.DrawString(_font, "Caravan Items", merchantTitlePos, Color.White);
            
            int yOffset = 310;
            int index = 0;
            foreach (var item in _currentTrader.Items)
            {
                Color itemColor = index == _selectedItemIndex ? Color.Yellow : Color.White;
                string itemText = $"{item.Key.Name} (x{item.Value}) - {item.Key.BasePrice}g";
                Vector2 itemPos = new Vector2(300, yOffset);
                _spriteBatch.DrawString(_font, itemText, itemPos, itemColor);
                yOffset += 25;
                index++;
            }
        }
        else
        {
            // Draw player inventory
            Vector2 playerTitlePos = new Vector2(300, 280);
            _spriteBatch.DrawString(_font, "Your Items", playerTitlePos, Color.White);
            
            int yOffset = 310;
            int index = 0;
            foreach (var item in _playerInventory.Items)
            {
                Color itemColor = index == _selectedItemIndex ? Color.Yellow : Color.White;
                string itemText = $"{item.Key.Name} (x{item.Value}) - {(int)(item.Key.BasePrice * 0.8f)}g";
                Vector2 itemPos = new Vector2(300, yOffset);
                _spriteBatch.DrawString(_font, itemText, itemPos, itemColor);
                yOffset += 25;
                index++;
            }
        }

        // Draw player's gold
        string goldText = $"Your Gold: {_playerInventory.Gold}g";
        Vector2 goldPos = new Vector2(backgroundRect.Center.X, backgroundRect.Bottom - 30);
        Vector2 goldSize = _font.MeasureString(goldText);
        _spriteBatch.DrawString(_font, goldText, goldPos - new Vector2(goldSize.X / 2, 0), Color.Gold);

        _spriteBatch.End();
    }

    // Helper method to draw a filled rectangle
    private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        if (_pixelTexture == null)
        {
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        spriteBatch.Draw(_pixelTexture, rectangle, color);
    }

    private void DrawWorldMap()
    {
        // Add null check for _worldMapTexture
        if (_worldMapTexture != null)
        {
            _spriteBatch.Draw(_worldMapTexture, Vector2.Zero, Color.White);
        }
        else
        {
            // Draw a placeholder background if texture is missing
            GraphicsDevice.Clear(Color.DarkGreen);
        }

        // Draw locations
        if (_locations != null)
        {
            foreach (var location in _locations)
            {
                if (location?.Icon != null)
                {
                    _spriteBatch.Draw(location.Icon, 
                        location.Position - new Vector2(location.Icon.Width / 2, location.Icon.Height / 2), 
                        Color.White);
                }
            }
        }

        // Draw caravans
        _caravanManager?.Draw(_spriteBatch, _caravanTexture);
        
        // Draw player
        _spriteBatch.Draw(_playerSprite, 
            _playerPosition - new Vector2(_playerSprite.Width / 2, _playerSprite.Height / 2), 
            Color.White);
        
        // Draw UI elements
        DrawWorldMapUI();
    }

    private Texture2D CreatePlaceholderTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(graphicsDevice, width, height);
        Color[] data = new Color[width * height];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = color;
        }
        texture.SetData(data);
        return texture;
    }

    private void InitializeLocations()
    {
        _locations = new List<Location>();
        // Add some default locations
        _locations.Add(new Location("Capital City", LocationType.City, 
            new Vector2(500, 500), _cityIcon));
        _locations.Add(new Location("Forest Village", LocationType.Village, 
            new Vector2(800, 300), _villageIcon));
        _locations.Add(new Location("Mountain Fortress", LocationType.Castle, 
            new Vector2(300, 700), _castleIcon));
    }

    private void InitializeQuests()
    {
        _activeQuests = new List<Quest>();
        // Add initial quests here if needed
    }

    private void InitializeReputation()
    {
        _reputation = new Dictionary<Location, int>();
        foreach (var location in _locations)
        {
            _reputation[location] = 0;
        }
    }

    private void HandleTerrainEffects(string terrainType)
    {
        // Modify player/party stats based on terrain
        float speedModifier = terrainType switch
        {
            "Plains" => 1.0f,
            "Forest" => 0.8f,
            "Mountains" => 0.6f,
            "Swamp" => 0.5f,
            _ => 1.0f
        };
        _playerSpeed = BASE_MOVEMENT_SPEED * speedModifier;
    }

    private void UpdateGathering(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _gatheringProgress += deltaTime;
        
        if (_gatheringProgress >= GATHERING_TIME)
        {
            _isGathering = false;
            _gatheringProgress = 0f;
            // Add gathered resources to inventory
            _gatheringResult = "Gathered resources!";
            _gatheringResultTimer = 2f;
        }
    }

    private void UpdateNearestLocation()
    {
        float nearestDistance = float.MaxValue;
        foreach (var location in _locations)
        {
            float distance = Vector2.Distance(_playerPosition, location.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                _nearestLocation = location;
            }
        }
    }

    private void DrawLocationMenu(GameTime gameTime)
    {
        _spriteBatch.Begin();
        // Draw menu options
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, "1. Trade", new Vector2(100, 100), Color.White);
            _spriteBatch.DrawString(_font, "2. Rest", new Vector2(100, 130), Color.White);
            _spriteBatch.DrawString(_font, "3. Recruit", new Vector2(100, 160), Color.White);
        }
        _spriteBatch.End();
    }

    private void DrawTrading(GameTime gameTime)
    {
        _spriteBatch.Begin();
        // Draw trading interface
        if (_font != null && _nearestLocation != null)
        {
            _spriteBatch.DrawString(_font, "Trading", new Vector2(100, 50), Color.Gold);
            // Add more trading UI elements
        }
        _spriteBatch.End();
    }

    private void DrawRecruiting(GameTime gameTime)
    {
        _spriteBatch.Begin();
        // Draw recruiting interface
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, "Recruiting", new Vector2(100, 50), Color.Gold);
            // Add more recruiting UI elements
        }
        _spriteBatch.End();
    }

    private void DrawBattle(GameTime gameTime)
    {
        _spriteBatch.Begin();
        // Draw battle interface
        if (_font != null && _currentBattle != null)
        {
            _spriteBatch.DrawString(_font, "Battle", new Vector2(100, 50), Color.Red);
            // Add more battle UI elements
        }
        _spriteBatch.End();
    }

    private void DrawWorldMapUI()
    {
        if (_font != null)
        {
            // Draw game time
            string timeString = $"Time: {(int)_gameTime:D2}:00";
            _spriteBatch.DrawString(_font, timeString, new Vector2(10, 10), Color.White);
            
            // Draw nearest location if close enough
            if (_nearestLocation != null)
            {
                float distance = Vector2.Distance(_playerPosition, _nearestLocation.Position);
                if (distance < _nearestLocation.InteractionRadius)
                {
                    string locationText = $"Press E to interact with {_nearestLocation.Name}";
                    _spriteBatch.DrawString(_font, locationText, 
                        new Vector2(10, _graphics.PreferredBackBufferHeight - 30), Color.White);
                }
            }
        }
    }
}
