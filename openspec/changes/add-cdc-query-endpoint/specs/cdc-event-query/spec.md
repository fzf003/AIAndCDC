## ADDED Requirements

### Requirement: Query CDC event list
The system SHALL expose an endpoint that returns a paginated list of received CDC events.

#### Scenario: List events with pagination
- **WHEN** client sends `GET /api/events?page=1&size=20`
- **THEN** system returns a JSON object containing `items` (array of events), `total` (total count), `page`, and `size`

#### Scenario: Filter events by operation type
- **WHEN** client sends `GET /api/events?op=c`
- **THEN** system returns only events where `op` equals "c" (create)

#### Scenario: Empty event list
- **WHEN** client sends `GET /api/events` and no events have been received yet
- **THEN** system returns `items` as an empty array and `total` as 0

### Requirement: Query single CDC event
The system SHALL expose an endpoint that returns a single CDC event by its identifier.

#### Scenario: Get existing event
- **WHEN** client sends `GET /api/events/{id}` for a valid event id
- **THEN** system returns the event details as JSON

#### Scenario: Get non-existing event
- **WHEN** client sends `GET /api/events/{id}` for an invalid id
- **THEN** system returns HTTP 404 with an error message

### Requirement: Query CDC event statistics
The system SHALL expose an endpoint that returns aggregated statistics of received CDC events.

#### Scenario: Get statistics
- **WHEN** client sends `GET /api/stats`
- **THEN** system returns a JSON object containing `totalEvents` and `byOp` (object with counts per operation type)

#### Scenario: Statistics with no events
- **WHEN** client sends `GET /api/stats` and no events have been received
- **THEN** system returns `totalEvents` as 0 and `byOp` as an empty object

### Requirement: Store received CDC events
The system SHALL retain received CDC events in memory for subsequent querying.

#### Scenario: Event is stored on receive
- **WHEN** a CDC event is posted to `POST /cdc`
- **THEN** the event is parsed and stored in memory with a unique identifier

#### Scenario: Memory retention limit
- **WHEN** more than 1000 events have been received
- **THEN** oldest events are discarded to maintain the 1000-event limit
