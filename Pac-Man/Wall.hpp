#pragma once
#include "Field.hpp"
#include <iostream>

// Klasa Wall – œciana w grze.
class Wall : public Field {
public:
    Wall(float x, float y, float tileSize)
        : Field(x, y, tileSize)
    {
        if (!textureLoaded) {
            if (!texture.loadFromFile("wall.png")) {
                std::cerr << "Failed to load wall texture!" << std::endl;
            }
            textureLoaded = true;
        }
        sprite.setTexture(texture);
        sprite.setPosition(x * tileSize, y * tileSize);
    }

    void draw(sf::RenderWindow& window) override {
        window.draw(sprite);
    }

    void onPlayerEnter() override {
    }

    void onGhostEnter() override {
    }

    bool isWalkable() const override {
        return false;
    }

private:
    static sf::Texture texture;
    static bool textureLoaded;
};

inline sf::Texture Wall::texture;
inline bool Wall::textureLoaded = false;
