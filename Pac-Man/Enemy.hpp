#pragma once
#include <SFML/Graphics.hpp>
#include <vector>
#include <mutex>
#include <iostream>
#include <cmath>
#include <random>

class Enemy {
public:
    Enemy(int tileX, int tileY, int tileSize)
        : tileSize(tileSize),
        cooldownTime(5.f),
        timeSinceEaten(0.f),
        moveTimer(0.f)
    {
        position = sf::Vector2f(tileX * static_cast<float>(tileSize),
            tileY * static_cast<float>(tileSize));
        startPosition = position;
        targetPosition = position;

        if (!texture.loadFromFile("enemy.png")) {
            std::cerr << "Failed to load enemy texture!" << std::endl;
        }
        sprite.setTexture(texture);
        sprite.setPosition(position);
        sprite.setScale(
            float(tileSize) / texture.getSize().x,
            float(tileSize) / texture.getSize().y
        );

        std::random_device rd;
        rng.seed(rd());

        speed = 80.f;
    }

    ~Enemy() = default;
    Enemy(const Enemy&) = delete;
    Enemy& operator=(const Enemy&) = delete;

    void update(float deltaTime, const std::vector<std::vector<char>>& map) {
        moveTimer += deltaTime;
        if (moveTimer >= moveInterval) {
            chooseNewDirection(map);
            moveTimer = 0.f;
        }

        sf::Vector2f dir = targetPosition - position;
        float dist = std::sqrt(dir.x * dir.x + dir.y * dir.y);
        if (dist > 1.f) {
            sf::Vector2f norm = dir / dist;
            float moveX = norm.x * speed * deltaTime;
            float moveY = norm.y * speed * deltaTime;

            float newX = position.x + moveX;
            if (!willCollide(newX, position.y, map)) {
                position.x = newX;
            }
            float newY = position.y + moveY;
            if (!willCollide(position.x, newY, map)) {
                position.y = newY;
            }
        }
        else {
            position = targetPosition;
        }

        sprite.setPosition(position);

        if (timeSinceEaten > 0.f) {
            timeSinceEaten -= deltaTime;
        }
    }

    void draw(sf::RenderWindow& window) {
        window.draw(sprite);
    }

    sf::FloatRect getBounds() const {
        return sprite.getGlobalBounds();
    }

    void resetPosition() {
        std::lock_guard<std::mutex> lock(positionMutex);
        position = startPosition;
        targetPosition = startPosition;
        sprite.setPosition(position);
        timeSinceEaten = cooldownTime;
    }

    bool canBeEaten() const {
        return timeSinceEaten <= 0.f;
    }

private:
    // Losowy wybór nowego kierunku – tylko kierunki, które nie powoduj¹ kolizji.
    void chooseNewDirection(const std::vector<std::vector<char>>& map) {
        std::vector<sf::Vector2f> validDirs;
        std::vector<sf::Vector2f> directions = {
            { static_cast<float>(tileSize),  0.f },
            { -static_cast<float>(tileSize), 0.f },
            { 0.f,  static_cast<float>(tileSize) },
            { 0.f, -static_cast<float>(tileSize) }
        };
        for (auto& d : directions) {
            float checkX = position.x + d.x;
            float checkY = position.y + d.y;
            if (!willCollide(checkX, checkY, map)) {
                validDirs.push_back(d);
            }
        }
        if (!validDirs.empty()) {
            std::uniform_int_distribution<int> dist(0, (int)validDirs.size() - 1);
            int idx = dist(rng);
            {
                std::lock_guard<std::mutex> lock(positionMutex);
                targetPosition = position + validDirs[idx];
            }
        }
    }

    // Sprawdza kolizjê przy u¿yciu zmniejszonego bounding boxa (margines 20% tileSize)
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

    sf::Vector2f startPosition;
    sf::Vector2f position;
    sf::Vector2f targetPosition;
    int tileSize;
    float speed;

    sf::Texture texture;
    sf::Sprite sprite;
    std::mutex positionMutex;

    float cooldownTime;
    float timeSinceEaten;

    float moveTimer;
    const float moveInterval = 0.5f;

    std::mt19937 rng;
};
