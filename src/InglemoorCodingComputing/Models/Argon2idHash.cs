namespace InglemoorCodingComputing.Models;

public record Argon2idHash(byte[] Hash, byte[] Salt, int Iterations, int Parallelism, int Memory);
