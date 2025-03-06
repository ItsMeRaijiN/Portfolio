#pragma once
#include <SFML/Graphics.hpp>
#include <vector>
#include <string>
#include <iostream>
#include <fstream>
#include <sstream>
#include <memory>
#include "Field.hpp"
#include "Wall.hpp"
#include "Point.hpp"
#include "Item.hpp"
#include "SuperItem.hpp"
#include "Banana.hpp"
#include "Peach.hpp"

// Klasa Maze – przechowuje mapê i pola (Field).
class Maze {
public:
    bool loadMap(const std::string& filename) {
        std::ifstream file(filename);
        if (!file.is_open()) {
            std::cerr << "Failed to open map file!" << std::endl;
            return false;
        }

        mapData.clear();
        ghostPositions.clear();
        fields.clear();

        std::string line;
        int y = 0;
        while (std::getline(file, line)) {
            std::vector<char> row;
            for (int x = 0; x < (int)line.size(); ++x) {
                char c = line[x];
                row.push_back(c);
                if (c == 'G') {
                    ghostPositions.emplace_back((float)x, (float)y);
                }
            }
            mapData.push_back(row);
            y++;
        }
        file.close();

        createFields();
        return true;
    }

    void draw(sf::RenderWindow& window) {
        for (auto& f : fields) {
            f->draw(window);
        }
    }

    // Znajduje pozycjê startow¹ gracza
    std::pair<int, int> findPlayerStartPosition() const {
        for (int y = 0; y < (int)mapData.size(); ++y) {
            for (int x = 0; x < (int)mapData[y].size(); ++x) {
                if (mapData[y][x] == 'P') {
                    return { x, y };
                }
            }
        }
        return { -1, -1 };
    }

    // Zwraca surowe dane mapy
    std::vector<std::vector<char>> getMap() const {
        return mapData;
    }

    // Zwraca wektor pozycji duchów
    const std::vector<sf::Vector2f>& getGhostPositions() const {
        return ghostPositions;
    }

    // Zwraca pola
    std::vector<std::unique_ptr<Field>>& getFields() {
        return fields;
    }

private:
    void createFields() {
        fields.clear();
        const float tileSize = 32.f;

        for (int y = 0; y < (int)mapData.size(); ++y) {
            for (int x = 0; x < (int)mapData[y].size(); ++x) {
                char c = mapData[y][x];
                switch (c) {
                case '#':
                    fields.push_back(std::make_unique<Wall>(x, y, tileSize));
                    break;
                case '.':
                    fields.push_back(std::make_unique<Point>(x, y, tileSize));
                    break;
                case 'X':
                    fields.push_back(std::make_unique<Item>(x, y, tileSize));
                    break;
                case 'Z':
                    fields.push_back(std::make_unique<SuperItem>(x, y, tileSize));
                    break;
                case 'B':
                    fields.push_back(std::make_unique<Banana>(x, y, tileSize));
                    break;
                case 'C':
                    fields.push_back(std::make_unique<Peach>(x, y, tileSize));
                    break;
                default:
                    // P, G, spacje itp.
                    break;
                }
            }
        }
    }

    std::vector<std::vector<char>> mapData;
    std::vector<std::unique_ptr<Field>> fields;
    std::vector<sf::Vector2f> ghostPositions;
};
