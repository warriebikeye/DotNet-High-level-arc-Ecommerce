1. AuthService
Description
Manages user registration, login, and email verification.
Stores user data in SQL Server.
Uses JWT for authentication.
Technologies
SQL Server: Relational database for user data.
Dapper: Lightweight ORM for SQL operations.
Serilog: Logging for tracking events.
SMTP: For sending verification emails.


2. ProductService
Description
Manages product catalog data using MongoDB, which is suitable for handling flexible, document-based schemas.

Technologies
MongoDB: NoSQL database for flexible, hierarchical data storage.
Serilog: For structured logging.

3. OrderService
Description
The OrderService is responsible for managing orders. It supports:

Order Creation: Insert orders into a SQL database.
Background Processing: Publish order creation events to Kafka for asynchronous processing.
Order Retrieval: Fetch order details.
Logging: Track all significant operations.
Technologies
SQL Server: For reliable storage of structured order data.
Kafka: For handling background tasks and event-driven architecture.
Dapper: For lightweight and efficient database access.
Serilog: For structured logging.
Background Service: For processing Kafka messages.

4. InventoryService
Description
The InventoryService will manage the stock levels of products. It will store the inventory count in Redis, allowing for extremely fast reads and writes. Additionally, it will track product stock adjustments and send updates asynchronously.

Technologies
Redis: In-memory data store for fast access to inventory counts.
Kafka: For handling inventory update events.
Serilog: For structured logging.
Background Service: For processing Kafka messages related to stock changes.


5. ReviewService
Description
The ReviewService will manage user reviews on products. It will support:

Adding Reviews: Users can post reviews for products.
Fetching Reviews: Fetch reviews for a specific product.
Handling High Write Loads: By using Cassandra, which is optimized for high throughput and horizontal scaling.
Logging: Track significant operations, such as adding or retrieving reviews.
Kafka: For asynchronous processing and event-driven architecture.
Technologies
Cassandra: A distributed NoSQL database for high write throughput.
Kafka: For handling events related to reviews.
Serilog: For structured logging.
Background Service: For processing Kafka events (e.g., processing reviews after being posted).
