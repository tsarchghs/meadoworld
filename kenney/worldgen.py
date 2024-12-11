from PIL import Image
import os
import random
import noise
import numpy as np
import heapq

class WorldGenerator:
    def __init__(self, tiles_path, output_size=(100, 100), tile_size=16):
        self.tiles_path = tiles_path
        self.output_size = output_size
        self.tile_size = tile_size
        self.tiles = {
            'snow': [],
            'tundra': [],
            'grass': [],
            'sand': [],
            'water': [],
            'mountain': [],
            'forest': [],
            'road': [],
            'settlement': {
                'village': [],    # Small settlements
                'city': [],       # Larger settlements
                'outpost': []     # Military/trading posts
            },
            'river': [],
            'coast': [],          # Coastal tiles
            'resources': {
                'metal': [],      # Metal deposits
                'gems': [],       # Gem deposits
                'wood': []        # Special forest resources
            }
        }
        self.load_tiles()
        
    def load_tiles(self):
        terrain_mapping = {
            # Base terrain
            'snow': ['0001'],
            'tundra': ['0000'],
            'grass': ['0000', '0001'],
            'sand': ['0001'],
            'water': ['0002', '0003'],
            'mountain': ['0020', '0021', '0022'],
            'forest': ['0016', '0017'],
            'road': ['0040', '0041'],
            'coast': ['0002'],  # Shallow water tiles
            # Settlements
            'settlement': {
                'village': ['0076'],
                'city': ['0077', '0078'],
                'outpost': ['0079']
            },
            # Resources
            'resources': {
                'metal': ['0090'],
                'gems': ['0091'],
                'wood': ['0016']
            }
        }

        # Load base terrain tiles
        for terrain, tile_numbers in terrain_mapping.items():
            if isinstance(tile_numbers, list):
                for number in tile_numbers:
                    tile_path = os.path.join(self.tiles_path, f'tile_{number}.png')
                    if os.path.exists(tile_path):
                        if terrain not in self.tiles:
                            self.tiles[terrain] = []
                        self.tiles[terrain].append(Image.open(tile_path))
            elif isinstance(tile_numbers, dict):
                # Handle nested dictionaries (settlements and resources)
                for subtype, subnumbers in tile_numbers.items():
                    for number in subnumbers:
                        tile_path = os.path.join(self.tiles_path, f'tile_{number}.png')
                        if os.path.exists(tile_path):
                            self.tiles[terrain][subtype].append(Image.open(tile_path))

    def generate_noise_map(self, scale=100.0, octaves=6):
        noise_map = np.zeros(self.output_size)
        for y in range(self.output_size[1]):
            for x in range(self.output_size[0]):
                noise_map[x][y] = noise.pnoise2(
                    x/scale, 
                    y/scale, 
                    octaves=octaves, 
                    persistence=0.5, 
                    lacunarity=2.0, 
                    repeatx=self.output_size[0], 
                    repeaty=self.output_size[1], 
                    base=random.randint(0, 1000)
                )
        return noise_map

    def get_biome(self, elevation, temperature):
        if elevation > 0.6:
            return 'mountain'
        if elevation < -0.2:
            return 'water'
        
        if temperature > 0.7:
            return 'sand'
        elif temperature < -0.3:
            return 'snow'
        elif temperature < 0:
            return 'tundra'
        elif random.random() < 0.3:  # 30% chance of forest in temperate areas
            return 'forest'
        else:
            return 'grass'

    def generate_rivers(self, elevation_map, num_rivers=10):
        rivers = set()
        mountain_points = []
        
        # Find mountain points as river sources
        for x in range(self.output_size[0]):
            for y in range(self.output_size[1]):
                if elevation_map[x][y] > 0.6:
                    mountain_points.append((x, y))
        
        # Generate rivers from random mountain points
        for _ in range(min(num_rivers, len(mountain_points))):
            if not mountain_points:
                break
                
            start = random.choice(mountain_points)
            mountain_points.remove(start)
            current = start
            river = {start}
            
            # River flows to lowest neighbor until reaching water
            while elevation_map[current[0]][current[1]] > -0.2:
                neighbors = []
                for dx, dy in [(0, 1), (1, 0), (0, -1), (-1, 0)]:
                    nx, ny = current[0] + dx, current[1] + dy
                    if (0 <= nx < self.output_size[0] and 
                        0 <= ny < self.output_size[1]):
                        neighbors.append((nx, ny, elevation_map[nx][ny]))
                
                if not neighbors:
                    break
                    
                # Move to lowest neighbor
                next_point = min(neighbors, key=lambda x: x[2])
                current = (next_point[0], next_point[1])
                river.add(current)
                
            rivers.update(river)
        
        return rivers

    def find_path(self, start, end, elevation_map):
        def heuristic(a, b):
            # Ensure a and b are tuples
            return abs(a[0] - b[0]) + abs(a[1] - b[1])
        
        def get_neighbors(pos):
            neighbors = []
            for dx, dy in [(0, 1), (1, 0), (0, -1), (-1, 0), 
                          (1, 1), (-1, 1), (1, -1), (-1, -1)]:
                nx, ny = pos[0] + dx, pos[1] + dy
                if (0 <= nx < self.output_size[0] and 
                    0 <= ny < self.output_size[1]):
                    # Higher cost for diagonal movement and elevation
                    cost = 1.4 if dx != 0 and dy != 0 else 1.0
                    cost += elevation_map[nx][ny] * 2  # Penalty for elevation
                    neighbors.append(((nx, ny), cost))  # Return position as tuple
            return neighbors
        
        # A* pathfinding
        frontier = [(0, start)]
        came_from = {start: None}
        cost_so_far = {start: 0}
        
        while frontier:
            current_cost, current = heapq.heappop(frontier)
            
            if current == end:
                break
            
            for next_pos, cost in get_neighbors(current):
                new_cost = cost_so_far[current] + cost
                
                if next_pos not in cost_so_far or new_cost < cost_so_far[next_pos]:
                    cost_so_far[next_pos] = new_cost
                    priority = new_cost + heuristic(next_pos, end)
                    heapq.heappush(frontier, (priority, next_pos))
                    came_from[next_pos] = current
        
        # Reconstruct path
        path = []
        current = end
        while current is not None:
            path.append(current)
            current = came_from.get(current)
        path.reverse()
        return path

    def generate_resources(self, elevation_map, temperature_map):
        resources = {}
        
        # Generate resource deposits based on terrain
        for x in range(self.output_size[0]):
            for y in range(self.output_size[1]):
                if elevation_map[x][y] > 0.5:  # Mountains
                    if random.random() < 0.1:  # 10% chance
                        resources[(x, y)] = 'metal'
                    elif random.random() < 0.05:  # 5% chance
                        resources[(x, y)] = 'gems'
                elif elevation_map[x][y] > 0 and temperature_map[x][y] > 0:  # Forested areas
                    if random.random() < 0.15:  # 15% chance
                        resources[(x, y)] = 'wood'
        
        return resources

    def determine_settlement_type(self, x, y, elevation_map, temperature_map):
        # Determine settlement type based on location and surrounding resources
        if elevation_map[x][y] > 0.4:  # Higher elevation
            return 'outpost'
        elif temperature_map[x][y] > 0.3:  # Warmer regions
            return 'city' if random.random() < 0.3 else 'village'
        else:
            return 'village'

    def generate_coastal_features(self, elevation_map):
        coastal_tiles = set()
        
        # Find coastal tiles (where land meets water)
        for x in range(self.output_size[0]):
            for y in range(self.output_size[1]):
                if elevation_map[x][y] > -0.2:  # Land
                    # Check neighbors for water
                    for dx, dy in [(0, 1), (1, 0), (0, -1), (-1, 0)]:
                        nx, ny = x + dx, y + dy
                        if (0 <= nx < self.output_size[0] and 
                            0 <= ny < self.output_size[1] and 
                            elevation_map[nx][ny] < -0.2):
                            coastal_tiles.add((x, y))
                            break
        
        return coastal_tiles

    def generate_world(self):
        # Create the world image
        world_width = self.output_size[0] * self.tile_size
        world_height = self.output_size[1] * self.tile_size
        world_map = Image.new('RGBA', (world_width, world_height), (0, 0, 0, 0))

        # Generate base maps
        elevation_map = self.generate_noise_map(scale=100.0)
        temperature_map = self.generate_noise_map(scale=100.0)
        
        # Generate features
        rivers = self.generate_rivers(elevation_map, num_rivers=10)
        coastal_tiles = self.generate_coastal_features(elevation_map)
        resources = self.generate_resources(elevation_map, temperature_map)
        
        # Generate settlements with types
        settlement_points = {}
        for _ in range(random.randint(8, 12)):
            while True:
                x = random.randint(0, self.output_size[0]-1)
                y = random.randint(0, self.output_size[1]-1)
                if (elevation_map[x][y] > -0.2 and 
                    elevation_map[x][y] < 0.6):
                    settlement_type = self.determine_settlement_type(
                        x, y, elevation_map, temperature_map
                    )
                    settlement_points[(x, y)] = settlement_type
                    break

        # Generate terrain with all features
        for y in range(self.output_size[1]):
            for x in range(self.output_size[0]):
                # Get the appropriate tile based on terrain type
                if (x, y) in rivers:
                    tile = random.choice(self.tiles['river'])
                elif (x, y) in coastal_tiles:
                    tile = random.choice(self.tiles['coast'])
                elif (x, y) in settlement_points:
                    settlement_type = settlement_points[(x, y)]
                    tile = random.choice(self.tiles['settlement'][settlement_type])
                elif (x, y) in resources:
                    resource_type = resources[(x, y)]
                    tile = random.choice(self.tiles['resources'][resource_type])
                else:
                    elevation = elevation_map[x][y]
                    temperature = temperature_map[x][y]
                    biome = self.get_biome(elevation, temperature)
                    tile = random.choice(self.tiles[biome])

                # Convert tile to RGBA if it isn't already
                if tile.mode != 'RGBA':
                    tile = tile.convert('RGBA')
                
                try:
                    world_map.paste(
                        tile,
                        (x * self.tile_size, y * self.tile_size),
                        tile  # Use the tile itself as the mask
                    )
                except ValueError:
                    # Fallback: paste without transparency if mask fails
                    world_map.paste(
                        tile,
                        (x * self.tile_size, y * self.tile_size)
                    )

        # Generate roads between settlements with same error handling
        settlement_locations = list(settlement_points.keys())
        for i in range(len(settlement_locations)-1):
            path = self.find_path(
                settlement_locations[i],
                settlement_locations[i+1],
                elevation_map
            )
            for x, y in path:
                if self.tiles['road']:
                    tile = random.choice(self.tiles['road'])
                    if tile.mode != 'RGBA':
                        tile = tile.convert('RGBA')
                    try:
                        world_map.paste(
                            tile,
                            (x * self.tile_size, y * self.tile_size),
                            tile
                        )
                    except ValueError:
                        world_map.paste(
                            tile,
                            (x * self.tile_size, y * self.tile_size)
                        )

        return world_map

    def save_world(self, filename="world_map.png"):
        world_map = self.generate_world()
        world_map.save(filename)

# Usage
if __name__ == "__main__":
    tiles_path = "Tiles"
    generator = WorldGenerator(
        tiles_path=tiles_path,
        output_size=(230, 230),  # Mount & Blade: Warband map size
        tile_size=16
    )
    generator.save_world("warband_sized_world.png")