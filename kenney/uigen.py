from PIL import Image, ImageDraw, ImageFont
import os

def create_ui_button(width, height, text, color=(52, 152, 219), hover=False):
    """Create a button with Kenney.nl style"""
    # Create base image
    img = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Colors
    if hover:
        top_color = tuple(min(c + 20, 255) for c in color)
        bottom_color = color
    else:
        top_color = color
        bottom_color = tuple(max(c - 20, 0) for c in color)
    
    # Draw button body (main rectangle)
    draw.rectangle([(0, 2), (width-1, height-3)], fill=top_color)
    # Draw bottom part (shadow)
    draw.rectangle([(0, height-3), (width-1, height-1)], fill=bottom_color)
    
    # Draw borders
    border_color = tuple(max(c - 40, 0) for c in color)
    draw.rectangle([(0, 2), (width-1, height-1)], outline=border_color)
    
    return img

def create_navigation_ui(output_dir="NavigationUI"):
    """Generate Rimworld-style navigation UI elements"""
    os.makedirs(output_dir, exist_ok=True)
    print(f"Saving UI elements to: {os.path.abspath(output_dir)}")
    
    # Button dimensions
    standard_width = 120
    standard_height = 40
    
    # Colors (Kenney.nl style)
    colors = {
        'blue': (52, 152, 219),    # Primary
        'green': (46, 204, 113),   # Confirm
        'red': (231, 76, 60),      # Back/Cancel
        'orange': (230, 126, 34),  # Warning/Special
        'gray': (149, 165, 166)    # Neutral
    }
    
    # Generate buttons
    buttons = {
        'nav_btn_back.png': {
            'size': (100, standard_height),
            'color': colors['red']
        },
        'nav_btn_next.png': {
            'size': (100, standard_height),
            'color': colors['green']
        },
        'nav_btn_select.png': {
            'size': (standard_width, standard_height),
            'color': colors['blue']
        },
        'nav_btn_random.png': {
            'size': (standard_width, standard_height),
            'color': colors['orange']
        },
        'nav_btn_factions.png': {
            'size': (standard_width, standard_height),
            'color': colors['gray']
        },
        # Hover versions
        'nav_btn_back_hover.png': {
            'size': (100, standard_height),
            'color': colors['red'],
            'hover': True
        },
        'nav_btn_next_hover.png': {
            'size': (100, standard_height),
            'color': colors['green'],
            'hover': True
        },
        'nav_btn_select_hover.png': {
            'size': (standard_width, standard_height),
            'color': colors['blue'],
            'hover': True
        },
        'nav_btn_random_hover.png': {
            'size': (standard_width, standard_height),
            'color': colors['orange'],
            'hover': True
        },
        'nav_btn_factions_hover.png': {
            'size': (standard_width, standard_height),
            'color': colors['gray'],
            'hover': True
        }
    }
    
    # Create panel background
    panel_width = 800
    panel_height = 600
    panel = Image.new('RGBA', (panel_width, panel_height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(panel)
    
    # Draw panel background
    background_color = (41, 128, 185, 200)  # Semi-transparent blue
    draw.rectangle([(0, 0), (panel_width-1, panel_height-1)], 
                  fill=background_color,
                  outline=(52, 152, 219))
    
    # Save panel
    panel.save(os.path.join(output_dir, 'nav_panel_bg.png'))
    print("Saved: nav_panel_bg.png")
    
    # Generate and save all buttons
    for filename, properties in buttons.items():
        button = create_ui_button(
            properties['size'][0],
            properties['size'][1],
            filename.split('.')[0],
            properties['color'],
            properties.get('hover', False)
        )
        button.save(os.path.join(output_dir, filename))
        print(f"Saved: {filename}")

    # Modified preview section
    preview_width = 1920  # Standard widescreen width
    preview_height = 1080  # Standard widescreen height
    preview = Image.new('RGBA', (preview_width, preview_height), (40, 44, 52))  # Darker background
    
    # Place panel background - centered horizontally, slightly higher vertically
    panel_x = (preview_width - panel_width) // 2
    panel_y = 50  # Positioned higher to leave room for buttons
    preview.paste(panel, (panel_x, panel_y), panel)
    
    # Place buttons in a more spread out layout
    button_y = preview_height - 100  # Position buttons near bottom
    spacing = 30  # Increased spacing between buttons
    
    # Calculate total width of all non-hover buttons
    total_buttons_width = sum(
        properties['size'][0] + spacing 
        for filename, properties in buttons.items() 
        if not filename.endswith('_hover.png')
    ) - spacing  # Subtract last spacing
    
    # Center buttons horizontally
    current_x = (preview_width - total_buttons_width) // 2
    
    # Place non-hover buttons only
    for filename, properties in buttons.items():
        if not filename.endswith('_hover.png'):
            button = create_ui_button(
                properties['size'][0],
                properties['size'][1],
                filename.split('.')[0],
                properties['color']
            )
            preview.paste(button, (current_x, button_y), button)
            current_x += properties['size'][0] + spacing
    
    # Save preview
    preview.save(os.path.join(output_dir, 'nav_ui_preview.png'))
    print("Saved: nav_ui_preview.png")

if __name__ == "__main__":
    create_navigation_ui()
    print("Navigation UI elements generated successfully!")
