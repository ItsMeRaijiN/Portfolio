#pragma once
#include <SFML/Graphics.hpp>
#include <vector>
#include <string>
#include <iostream>
#include <cmath>

class Player {
public:
    Player(int startTileX, int startTileY, int tileSize)
        : tileSize(tileSize), speed(100.f), invincibleDuration(0.f)
    {
        posX = startTileX * static_cast<float>(tileSize);
        posY = startTileY * static_cast<float>(tileSize);

        startX = posX;
        startY = posY;

        if (!texture.loadFromFile("player.png")) {
            std::cerr << "Failed to load player texture!" << std::endl;
        }
        sprite.setTexture(texture);
        sprite.setPosition(posX, posY);

        velocity = sf::Vector2f(0.f, 0.f);
    }

    void setDirection(const sf::Vector2f& dir) {
        if (dir.x != 0.f || dir.y != 0.f) {
            float length = std::sqrt(dir.x * dir.x + dir.y * dir.y);
            sf::Vector2f norm = dir / length;
            velocity = norm * speed;
        }
        else {
            velocity = sf::Vector2f(0.f, 0.f);
        }
    }

    void update(float deltaTime, const std::vector<std::vector<char>>& map) {
        float newX = posX + velocity.x * deltaTime;
        if (!willCollide(newX, posY, map)) {
            posX = newX;
        }
        else {
            velocity.x = 0.f;
        }

        float newY = posY + velocity.y * deltaTime;
        if (!willCollide(posX, newY, map)) {
            posY = newY;
        }
        else {
            velocity.y = 0.f;
        }

        sprite.setPosition(posX, posY);

        if (speedBoostActive) {
            speedBoostTimer -= deltaTime;
            if (speedBoostTimer <= 0.f) {
                speed = 100.f;
                speedBoostActive = false;
            }
        }

        if (invincibleDuration > 0.f) {
            invincibleDuration -= deltaTime;
        }
    }

    void draw(sf::RenderWindow& window) {
        window.draw(sprite);
    }

    sf::FloatRect getBounds() const {
        return sprite.getGlobalBounds();
    }

    // Bonus: przyspieszenie
    void applySpeedBoost(float duration) {
        speed = 200.f;
        speedBoostTimer = duration;
        speedBoostActive = true;
    }

    // Bonus: nieœmiertelnoœæ
    void applyInvincibility(float duration) {
        invincibleDuration = duration;
    }

    bool isInvincible() const {
        return invincibleDuration > 0.f;
    }

    void resetPosition() {
        posX = startX;
        posY = startY;
        velocity = sf::Vector2f(0.f, 0.f);
        sprite.setPosition(posX, posY);
    }

private:
    // Funkcja willCollide – sprawdza, czy pozycja (x,y) spowoduje kolizjê.
    // U¿ywa zmniejszonego bounding boxa (margines = 20% tileSize) dla bardziej luŸnej kolizji.
    bool willCollide(float x, float y, const std::vector<std::vector<char>>& map) {
        sf::FloatRect bounds = sprite.getGlobalBounds();
        bounds.left = x;
        bounds.top = y;

        float margin = 0.2f * tileSize;
        bounds.left += margin;
        bounds.top += margin;
        bounds.width -= 2 * margin;
        bounds.height -= 2 * margin;

        int leftTile = static_cast<int>(std::floor(bounds.left / tileSize));
        int rightTile = static_cast<int>(std::floor((bounds.left + bounds.width) / tileSize));
        int topTile = static_cast<int>(std::floor(bounds.top / tileSize));
        int bottomTile = static_cast<int>(std::floor((bounds.top + bounds.height) / tileSize));

        for (int ty = topTile; ty <= bottomTile; ++ty) {
            for (int tx = leftTile; tx <= rightTile; ++tx) {
                if (ty < 0 || ty >= static_cast<int>(map.size()) ||
                    tx < 0 || tx >= static_cast<int>(map[ty].size()))
                {
                    return true; 
                }
                if (map[ty][tx] == '#') {
                    return true;
                }
            }
        }
        return false;
    }

    float posX, posY;     
    float startX, startY; 
    int tileSize;         

    float speed;          
    sf::Vector2f velocity;

    sf::Texture texture;
    sf::Sprite sprite;

    bool speedBoostActive = false;
    float speedBoostTimer = 0.f;

    float invincibleDuration;
};
