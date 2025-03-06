#pragma once
import Health;
#include <SFML/Graphics.hpp>
#include <vector>
#include <string>
#include <iostream>
#include <fstream>
#include <memory>
#include <filesystem>
#include "Player.hpp"
#include "Field.hpp"
#include "Maze.hpp"
#include "Enemy.hpp"
#include "Banana.hpp"
#include "Peach.hpp"
#include "Point.hpp"
#include "Item.hpp"
#include "SuperItem.hpp"
#include "Wall.hpp"

class Game {
public:
    Game(float width, float height)
        : windowWidth(width), windowHeight(height), tileSize(32),
        score(0), gameTime(300.f), gameEnded(false),
        playerName(""), isEndScreenActive(false),
        playerHealth(3, 900, 150, 32)
    {
        if (!font.loadFromFile("arial.ttf")) {
            std::cerr << "Failed to load font!" << std::endl;
        }

        scoreText.setFont(font);
        scoreText.setFillColor(sf::Color::Yellow);
        scoreText.setCharacterSize(50);
        scoreText.setPosition(900, 80);
        updateScoreText();

        timerText.setFont(font);
        timerText.setFillColor(sf::Color::Yellow);
        timerText.setCharacterSize(50);
        timerText.setPosition(900, 20);
        updateTimerText();

        rulesText.setFont(font);
        rulesText.setFillColor(sf::Color::White);
        rulesText.setCharacterSize(40);
        rulesText.setString("Welcome to Gaijin The Snail!\n\nUse arrow keys to move.\nEat all dollars to win.\nApple => speed boost\nCherry => invincibility\nAvoid ghosts!\n\nPress Enter to start the game.");
        rulesText.setPosition(sf::Vector2f(width / 2 - rulesText.getGlobalBounds().width / 2, height / 4));

        background.setSize(sf::Vector2f(width - 100, 450));
        background.setFillColor(sf::Color(0, 0, 0, 150));
        background.setPosition(50, height / 4 - 10);
    }

    ~Game() {
        resetEnemies();
    }

    void setPlayerName(const std::string& name) {
        playerName = name;
    }

    bool loadMap(const std::string& filename) {
        std::cout << "Loading map: " << filename << std::endl;
        if (!maze.loadMap(filename)) {
            return false;
        }
        auto [gx, gy] = maze.findPlayerStartPosition();
        if (gx == -1 || gy == -1) {
            std::cerr << "Player start position not found!" << std::endl;
            return false;
        }
        player = std::make_unique<Player>(gx, gy, tileSize);

        resetEnemies();
        for (auto& pos : maze.getGhostPositions()) {
            int tileX = (int)pos.x;
            int tileY = (int)pos.y;
            enemies.push_back(std::make_unique<Enemy>(tileX, tileY, tileSize));
        }
        return true;
    }

    void drawRules(sf::RenderWindow& window) {
        window.draw(background);
        window.draw(rulesText);
    }

    void draw(sf::RenderWindow& window) {
        if (gameEnded) {
            drawEndScreen(window);
        }
        else {
            maze.draw(window);
            if (player) player->draw(window);
            for (auto& e : enemies) {
                e->draw(window);
            }
            playerHealth.draw(window);
            window.draw(scoreText);
            window.draw(timerText);
        }
    }

    void updatePlayer(float deltaTime) {
        if (!gameEnded) {
            if (player) {
                player->update(deltaTime, maze.getMap());
            }
            for (auto& e : enemies) {
                e->update(deltaTime, maze.getMap());
            }
            collectPoint();
            collectItem();
            collectSuperItem();
            collectBanana();
            collectPeach();
            checkCollisions();
            gameTime -= deltaTime;
            if (gameTime <= 0.f) {
                gameTime = 0.f;
                gameEnded = true;
                isEndScreenActive = true;
            }
            updateTimerText();
        }
    }

    void movePlayerSmooth(const sf::Vector2f& dir) {
        if (!gameEnded && player) {
            player->setDirection(dir);
        }
    }

    bool hasEnded() const {
        return gameEnded;
    }

    void saveScore() {
        std::filesystem::path scoresFile = "scores.txt";
        std::ofstream outFile(scoresFile, std::ios::app);
        if (outFile.is_open()) {
            outFile << playerName << " " << score << "\n";
            outFile.close();
        }
        else {
            std::cerr << "Failed to open scores file!" << std::endl;
        }
    }

    void resetGame() {
        gameTime = 300.f;
        score = 0;
        playerHealth.reloadTextures();
        playerHealth.reset(3);
        gameEnded = false;
        isEndScreenActive = false;
        updateScoreText();
        updateTimerText();
        resetEnemies();
    }

private:
    void drawEndScreen(sf::RenderWindow& window) {
        sf::Text endText;
        endText.setFont(font);
        endText.setFillColor(sf::Color::Red);
        endText.setCharacterSize(50);
        endText.setString("Game Over\nYour Score: " + std::to_string(score) + "\nPlayer: " + playerName +
            "\nPress ENTER to save your score\nPress ESC to return to menu");
        endText.setPosition(sf::Vector2f(windowWidth / 2 - endText.getGlobalBounds().width / 2,
            windowHeight / 2 - 100));
        window.draw(endText);
    }

    void updateScoreText() {
        scoreText.setString("Score: " + std::to_string(score));
    }

    void updateTimerText() {
        int minutes = static_cast<int>(gameTime) / 60;
        int seconds = static_cast<int>(gameTime) % 60;
        timerText.setString("Time: " + std::to_string(minutes) + ":" + (seconds < 10 ? "0" : "") + std::to_string(seconds));
    }

    void collectPoint() {
        if (!player) return;
        auto playerBounds = player->getBounds();
        bool allPointsCollected = true;
        for (auto& field : maze.getFields()) {
            if (auto p = dynamic_cast<Point*>(field.get())) {
                if (!p->isCollected()) {
                    allPointsCollected = false;
                    if (playerBounds.intersects(p->getBounds())) {
                        p->onPlayerEnter();
                        score += 10;
                        updateScoreText();
                    }
                }
            }
        }
        if (allPointsCollected) {
            gameEnded = true;
            isEndScreenActive = true;
        }
    }

    void collectItem() {
        if (!player) return;
        auto playerBounds = player->getBounds();
        for (auto& field : maze.getFields()) {
            if (auto item = dynamic_cast<Item*>(field.get())) {
                if (!item->isCollected() && playerBounds.intersects(item->getBounds())) {
                    item->onPlayerEnter();
                    score += 250;
                    updateScoreText();
                    player->applySpeedBoost(5.f);
                }
            }
        }
    }

    void collectSuperItem() {
        if (!player) return;
        auto playerBounds = player->getBounds();
        for (auto& field : maze.getFields()) {
            if (auto sItem = dynamic_cast<SuperItem*>(field.get())) {
                if (!sItem->isCollected() && playerBounds.intersects(sItem->getBounds())) {
                    sItem->onPlayerEnter();
                    score += 1000;
                    updateScoreText();
                    player->applyInvincibility(5.f);
                }
            }
        }
    }

    void collectBanana() {
        if (!player) return;
        auto playerBounds = player->getBounds();
        for (auto& field : maze.getFields()) {
            if (auto banana = dynamic_cast<Banana*>(field.get())) {
                if (!banana->isCollected() && playerBounds.intersects(banana->getBounds())) {
                    banana->onPlayerEnter();
                    score += 50;
                    updateScoreText();
                }
            }
        }
    }

    void collectPeach() {
        if (!player) return;
        auto playerBounds = player->getBounds();
        for (auto& field : maze.getFields()) {
            if (auto peach = dynamic_cast<Peach*>(field.get())) {
                if (!peach->isCollected() && playerBounds.intersects(peach->getBounds())) {
                    peach->onPlayerEnter();
                    score += 100;
                    updateScoreText();
                }
            }
        }
    }

    void checkCollisions() {
        if (!player) return;
        auto playerBounds = player->getBounds();
        for (auto& e : enemies) {
            if (playerBounds.intersects(e->getBounds())) {
                if (player->isInvincible() && e->canBeEaten()) {
                    score += 500;
                    updateScoreText();
                    e->resetPosition();
                }
                else if (!player->isInvincible()) {
                    playerHealth.loseLife();
                    if (playerHealth.getLives() > 0) {
                        player->resetPosition();
                    }
                    else {
                        gameEnded = true;
                        isEndScreenActive = true;
                    }
                }
            }
        }
    }

    void resetEnemies() {
        for (auto& e : enemies) {
            e.reset();
        }
        enemies.clear();
    }

    float windowWidth, windowHeight;
    int tileSize;
    int score;
    float gameTime;
    bool gameEnded;
    bool isEndScreenActive;
    std::string playerName;

    Maze maze;
    std::unique_ptr<Player> player;
    std::vector<std::unique_ptr<Enemy>> enemies;

    sf::Font font;
    sf::Text scoreText;
    sf::Text timerText;
    sf::Text rulesText;
    sf::RectangleShape background;

    Health playerHealth;
};
