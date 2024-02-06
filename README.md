# Tic-Tac-Toe Game

A implementation of two different Tic-Tac-Toe Versions using Unity.


## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Getting Started](#getting-started)
- [Game Rules](#game-rules)

## Introduction

This project is a implementation of the classic Tic-Tac-Toe game and a Version of the Gobblet-Ruleset developed using Unity. The game allows two players to take turns marking spaces in a 3x3 grid, with the objective of forming a horizontal, vertical, or diagonal line of their marker.
Furthermore its possible to make each move using Command-Skrips in JSON-Format. This allows a connection to a separate python server to fully controll the turns. This could for example be used to digitally mirror robots playing physical TicTacToe and send each of their moves via the Command-Skripts to this unity application. 

## Features

- Player vs Player gameplay.
- Main- & Pausemenu
- Mouse and Skript controlls possible
- Read current state of board
- Simple and intuitive user interface.
- Win and tie detection.
- AI-Player using MiniMax (algorithm easily changeable) 

## Getting Started

### Prerequisites

- Unity (Version 2022.3.13f1)

### Installation

1. Clone the repository:

    ```bash
    git clone https://github.com/your-username/tic-tac-toe.git
    ```

2. Open the project in Unity.

3. Build and run the game.

## Game Rules

- The game is played on a 3x3 grid.
- Players take turns placing their markers (X or O) in empty spaces.
- The player who succeeds in placing three of their markers in a horizontal, vertical, or diagonal row wins.
