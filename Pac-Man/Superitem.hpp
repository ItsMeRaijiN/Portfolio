#pragma once
#include "Field.hpp"
#include <iostream>

// Klasa SuperItem – daje nieœmiertelnoœæ.
class SuperItem : public Field {
public:
    SuperItem(float x, float y, float tileSize)
        : Field(x, y, tileSize)
    {
        if (!textureLoaded) {
            if (!texture.loadFromFile("wisnie.png")) {
                std::cerr << "Failed to load super item texture!" << std::endl;
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

inline sf::Texture SuperItem::texture;
inline bool SuperItem::textureLoaded = false;
