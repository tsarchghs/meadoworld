from PIL import Image, ImageDraw
import os

def create_navigation_sprites(output_dir="Navigation", size=16):
    """Generate navigation-related sprites."""
    
    # Get absolute path and create Navigation directory
    abs_output_dir = os.path.abspath(output_dir)
    os.makedirs(abs_output_dir, exist_ok=True)
    print(f"Saving navigation sprites to: {abs_output_dir}")
    
    def create_base_image():
        """Create a transparent base image."""
        return Image.new('RGBA', (size, size), (0, 0, 0, 0))
    
    def create_path_indicator(color, alpha=128):
        """Create a path indicator dot/square."""
        img = create_base_image()
        draw = ImageDraw.Draw(img)
        padding = size // 4
        draw.ellipse([padding, padding, size-padding, size-padding], 
                    fill=(color[0], color[1], color[2], alpha))
        return img
    
    def create_x_mark(color, alpha=180):
        """Create an X mark for forbidden areas."""
        img = create_base_image()
        draw = ImageDraw.Draw(img)
        padding = size // 4
        # Draw X
        draw.line([(padding, padding), (size-padding, size-padding)], 
                 fill=(color[0], color[1], color[2], alpha), width=2)
        draw.line([(size-padding, padding), (padding, size-padding)], 
                 fill=(color[0], color[1], color[2], alpha), width=2)
        return img
    
    def create_selection_box(color, alpha=200):
        """Create a selection box."""
        img = create_base_image()
        draw = ImageDraw.Draw(img)
        padding = 1
        # Draw border
        draw.rectangle([padding, padding, size-padding-1, size-padding-1], 
                      outline=(color[0], color[1], color[2], alpha), width=1)
        return img
    
    def create_target_marker(color, alpha=200):
        """Create a target marker (crosshair)."""
        img = create_base_image()
        draw = ImageDraw.Draw(img)
        center = size // 2
        radius = size // 4
        # Draw crosshair
        draw.line([(center, center-radius), (center, center+radius)], 
                 fill=(color[0], color[1], color[2], alpha), width=2)
        draw.line([(center-radius, center), (center+radius, center)], 
                 fill=(color[0], color[1], color[2], alpha), width=2)
        # Draw circle
        draw.ellipse([center-radius, center-radius, center+radius, center+radius], 
                    outline=(color[0], color[1], color[2], alpha), width=1)
        return img
    
    def create_waypoint(color, alpha=180):
        """Create a waypoint marker (small diamond)."""
        img = create_base_image()
        draw = ImageDraw.Draw(img)
        center = size // 2
        radius = size // 4
        # Draw diamond
        points = [
            (center, center-radius),  # top
            (center+radius, center),  # right
            (center, center+radius),  # bottom
            (center-radius, center)   # left
        ]
        draw.polygon(points, fill=(color[0], color[1], color[2], alpha))
        return img

    # Ensure output directory exists
    os.makedirs(output_dir, exist_ok=True)

    # Generate sprites
    sprites = {
        # Path indicators
        'nav_path.png': create_path_indicator((255, 255, 255)),    # White path
        'nav_path_good.png': create_path_indicator((0, 255, 0)),   # Green path
        'nav_path_med.png': create_path_indicator((255, 255, 0)),  # Yellow path
        'nav_path_bad.png': create_path_indicator((255, 0, 0)),    # Red path
        
        # Special markers
        'nav_forbidden.png': create_x_mark((255, 0, 0)),           # Forbidden X
        'nav_allowed.png': create_path_indicator((0, 255, 0), 160),# Allowed area
        'nav_select.png': create_selection_box((0, 162, 232)),     # Selection box
        'nav_target.png': create_target_marker((255, 128, 0)),     # Target marker
        'nav_waypoint.png': create_waypoint((0, 162, 232))         # Waypoint
    }


    # Save all sprites
    for filename, sprite in sprites.items():
        full_path = os.path.join(abs_output_dir, filename)
        sprite.save(full_path)
        print(f"Saved: {filename}")

if __name__ == "__main__":
    create_navigation_sprites()
    print("Navigation sprites generated successfully!")