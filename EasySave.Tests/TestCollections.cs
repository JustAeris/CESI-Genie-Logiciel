namespace EasySave.Tests;

// Forces sequential execution for all test classes sharing Logger/StateManager singletons.
// xUnit parallelises across collections; tests inside the same collection run one-by-one.
[CollectionDefinition("Singletons")]
public class SingletonCollection;
