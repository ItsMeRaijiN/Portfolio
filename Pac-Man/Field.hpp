#pragma once
#include <SFML/Graphics.hpp>
#include <vector>

// Klasa bazowa Field – reprezentuje dowolne pole na planszy.
// Przechowujemy tutaj wspólne elementy: pozycjê, sprite, itp.
class Field {
public:
    // Konstruktor: x, y to wspó³rzêdne w kafelkach (map[y][x])
    Field(float x, float y, float tileSize)
        : tileX(x), tileY(y), tileSize(tileSize), collected(false)
    {
        sprite.setPosition(tileX * tileSize, tileY * tileSize);
    }

    virtual ~Field() = default;

    virtual void draw(sf::RenderWindow& window) = 0;

    virtual void onPlayerEnter() = 0;

    virtual void onGhostEnter() = 0;

    virtual bool isWalkable() const = 0;

    bool isCollected() const { return collected; }

    void collect() { collected = true; }

    // Granice sprite'a
    sf::FloatRect getBounds() const {
        return sprite.getGlobalBounds();
    }

protected:
    float tileX, tileY;      
    float tileSize;          
    bool collected;          
    sf::Sprite sprite;       
};
