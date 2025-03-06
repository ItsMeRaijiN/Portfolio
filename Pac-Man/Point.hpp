#pragma once
#include "Field.hpp"
#include <iostream>

// Klasa Point – pojedynczy punkt do zebrania.
class Point : public Field {
public:
    Point(float x, float y, float tileSize)
        : Field(x, y, tileSize)
    {
        if (!textureLoaded) {
            if (!texture.loadFromFile("point2.png")) {
                std::cerr << "Failed to load point texture!" << std::endl;
            }
            textureLoaded = true;
        }
        sprite.setTexture(texture);
        sprite.setPosition(x * tileSize, y * tileSize);
    }

    void draw(sf::RenderWindow& window) override {
        if (!collected) {
            window.draw(sprite);
        }
    }

    void onPlayerEnter() override {
        collected = true;
    }

    void onGhostEnter() override {
        // Duchy nie zbieraj¹ punktów
    }

    bool isWalkable() const override {
        return true;
    }

private:
    static sf::Texture texture;
    static bool textureLoaded;
};

inline sf::Texture Point::texture;
inline bool Point::textureLoaded = false;
