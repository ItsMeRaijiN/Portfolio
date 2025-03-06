#pragma once
#include "Field.hpp"
#include <iostream>

// Klasa Banana – dodatkowy przedmiot
class Banana : public Field {
public:
    Banana(float x, float y, float tileSize)
        : Field(x, y, tileSize)
    {
        if (!textureLoaded) {
            if (!texture.loadFromFile("banana.png")) {
                std::cerr << "Failed to load banana texture!" << std::endl;
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
        // Duch nie zbiera
    }

    bool isWalkable() const override {
        return true;
    }

private:
    static sf::Texture texture;
    static bool textureLoaded;
};

inline sf::Texture Banana::texture;
inline bool Banana::textureLoaded = false;
