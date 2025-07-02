# Purpose of this namespace

I propose to host here some common business rules or algorithms that get easily duplicated and are error-prone to find
and reflect the changes. Code in here should not be technical in nature, rather some executable logic, specific
constants not found in applications Configuration. Idea is to have utility/helper classes that have single
responsibility, mostly static/pure functions. With responsibility its is meant that one class should work over certain
part of business model and methods in it should be highly cohesive.

* use simple static classes, this allows direct use of them in consuming code;
* use names that clearly describe the purpose of every class or its members;
   * for class names you could use suffixes like "Algorithms", "Helpers", "Utils", this conveys a hint, that it is not 
required to define them as dependencies via constructor; 
* keep the methods concise and short, reduce dependencies, other types in this namespace should be OK, but the rest 
should be provided via parameters;
* feel free to host constants, but it should make sense to make them public;
* document, document, document, classes and members -- I cannot stress it enough, minimize the need to open the source,
  it saves time and describes the value;
* ideally, write unit tests, it should be su.per easy, if you've followed these guidelines, especially simplicity of 
 the logic
