﻿using DropSpace.Domain;
using DropSpace.Logic.Events.Interfaces;

namespace DropSpace.Logic.Events.Events
{
    public record UserJoinedEvent(Session Session, string UserId) : IEvent;
}
