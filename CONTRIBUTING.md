# Contributing to MixinSharp

First off, thank you for your interest in contributing to MixinSharp!
This project exists to make mixin-style development in C# simple, safe, and elegant.

---

## Guiding Principles

* **Clarity over cleverness:** Write code that is easy to read and reason about.
* **Safety first:** MixinSharp exists to *reduce* foot guns, not introduce new ones.
* **Predictability:** Generated code and compiler diagnostics should behave in consistent, transparent ways.
* **Kindness:** Respect other contributors’ time, perspectives, and experience levels. Code reviews are about collaboration, not competition.

---

## Getting Started

### 1. Fork & clone the repository

```bash
git clone https://github.com/<your-username>/MixinSharp.git
cd MixinSharp
```

### 2. Build the project

MixinSharp uses the standard .NET SDK workflow:

```bash
dotnet build
```

### 3. Run the tests

Tests are written using xUnit. To verify your changes:

```bash
dotnet test
```

If you're adding new behavior, please add or update relevant tests in `MixinSharp.Tests/`.

---

## Project Structure

```
/MixinSharp/            # Common classes and attributes to control the generator
/MixinSharp.Generators/  # Source generator and supporting logic
/MixinSharp.Samples/    # Test project for demonstrating features and how to use the library
/MixinSharp.Tests/      # Unit and integration tests
/MixinSharp/docs/       # Documentation and examples
```


---

## Code Style

* Follow **.NET naming conventions** (PascalCase for types/methods, camelCase for locals and parameters).
* Use **`var`** only when the type is obvious from the right-hand side.
* Prefer **expression-bodied members** when they improve clarity.
* Add XML documentation for all public types and members.
* Include meaningful Roslyn diagnostics with clear messages and unique IDs (e.g., `MXN001`, `MXN002`).

A consistent style helps others read and trust your work.

---

## Submitting a Pull Request

1. Create a new branch from `main`:

   ```bash
   git checkout -b feature/add-logger-mixin
   ```
2. Make your changes and commit them with clear messages.
3. Ensure all tests pass locally.
4. Submit a PR describing **what** you changed and **why**.
5. Be open to feedback, reviews are collaborative discussions.

---

## Reporting Issues

If you find a bug or have a feature idea, open an issue with:

* A clear, minimal reproduction if possible
* What behavior you expected vs. what occurred
* Your environment (OS, .NET SDK version, IDE)

---

## Communication & Community

Discussion happens through:

* GitHub Issues and Pull Requests
* Occasional design discussions in the Discussions tab

Everyone is welcome. The tone here should be professional, curious, and kind.

---

## License

By contributing, you agree that your contributions will be licensed under the same license as the project (MIT).

---

Thank you for helping shape MixinSharp!
