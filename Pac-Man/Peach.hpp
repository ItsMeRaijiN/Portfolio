#pragma once
#include "Field.hpp"
#include <iostream>

// Klasa Peach – inny rodzaj przedmiotu
class Peach : public Field {
public:
    Peach(float x, float y, float tileSize)
        : Field(x, y, tileSize)
    {
        if (!textureLoaded) {
            if (!texture.loadFromFile("peach.png")) {
                std::cerr << "Failed to load peach texture!" << std::endl;
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

inline sf::Texture Peach::texture;
inline bool Peach::textureLoaded = false;
