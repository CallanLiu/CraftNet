﻿using XGFramework;

namespace Test;

public record struct EventWebSocketConnected(Agent Agent) : IEvent;

public record struct EventWebSocketDisconnected(Agent Agent) : IEvent;