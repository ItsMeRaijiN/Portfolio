#pragma once
#include "Field.hpp"
#include <iostream>

// Klasa Item – zwyk³y przedmiot, daje bonus prêdkoœci.
class Item : public Field {
public:
    Item(float x, float y, float tileSize)
        : Field(x, y, tileSize)
    {
        if (!textureLoaded) {
            if (!texture.loadFromFile("JapkoXD.png")) {
                std::cerr << "Failed to load item texture!" << std::endl;
            }
            textureLoaded = true;
        }
        sprite.setTexture(texture);
        sprite.setPosition(x * tileSize, y * tileSize);
    }

    void draw(sf::RenderWindow& window) override {
        if (!isCollected()) {
            window.draw(sprite);
        }
    }

    void onPlayerEnter() override {
        collect();
    }

    void onGhostEnter() override {
        // Duchy nie zbieraj¹
    }

    bool isWalkable() const override {
        return true;
    }

private:
    static sf::Texture texture;
    static bool textureLoaded;
};

inline sf::Texture Item::texture;
inline bool Item::textureLoaded = false;
