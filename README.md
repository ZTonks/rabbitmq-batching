# priority-rabbitmq-consumers

POC to show how we can use a consumer pool to consume multiple RabbitMQ queues with [DWRR](https://en.wikipedia.org/wiki/Deficit_round_robin).

The `PriorityQueues.Consume` project shows how we can set up consumers, and `PriorityQueues.Produce` is a utility to produce messages.

The basic premise is to set up a `PrioritySlot` for each priority queue, and set the `Quantum` to determine weighting. If one priority slot has a `Quantum` of 2 and another has a `Quantum` of 1, the first slot will consume 2 messages from its queue for every 1 message the second slot consumes (provided there's enough messages in both queues).
