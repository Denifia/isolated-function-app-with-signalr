# Synchronize Isolated Function Apps via SignalR

## Table of Contents <!-- omit in toc -->
- [Motivation](#motivation)
- [Components](#components)

## Motivation

I want to have many instances of my function (both scaled out and deployed to different regions) be able to broadcast information to each other.

## Components

- Function App (must be on a dedicated plan; consumption/serverless wont work)
- Hosted Background Worker
- SignalR Services or the local emulator
