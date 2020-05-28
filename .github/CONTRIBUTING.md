# Contributing to Aurora

### I have found a bug!
- Please make sure that your bug was not already reported by searching the [issues](https://github.com/antonpup/Aurora/issues).
- If your bug was already reported and you have additional information to provide, please comment on the existing issue. Do not create a new issue that is a duplicate of an existing one (unless it has been closed).
- If you cannot find your issue reported, please make a [New Issue](https://github.com/antonpup/Aurora/issues/new) and follow the bug report template.

### I want to contribute code!
- Please first create your own fork of this repo if you do not already have one.
  - If you have not done so yet, you will need to clone your repo onto your local machine for development. `git clone https://github.com/your-username/Aurora.git`
  - You will also need to add the main repo as an upstream to get updated commits. `git remote add upstream https://github.com/antonpup/Aurora.git`
- Make sure your fork is up-to-date with the latest dev branch on the main repo. `git fetch upstream`
- Create a feature or fix branch from the latest dev branch. `git checkout -b feature/my-new-feature upstream/dev`
  - If you plan on adding new features, please prefix your branch name with 'feature/'. For example `feature/audio-layer-improvements`.
  - If you plan on fixing bugs, please prefix your branch name with 'fix/'. For example `fix/layer-selection`.
  - If you are adding features and bug fixes, use 'feature/'.
- If you are adding or fixing multiple unrelated areas, please divide your code into different branches. For example, don't add a new device at the same time as you change how the overrides system works.
- Please ensure that your code follows the existing conventions set out in the code (such as using auto properties where applicable, naming conventions, etc.).
- When you have finished your feature or bug fix, push your changes to your remote branch. E.g. `git push origin feature/my-cool-feature`
- Create a [new pull request](https://github.com/antonpup/Aurora/compare) on the main Aurora repo.
- Add a descriptive title and description which includes a detailed list of what has been changed and any known issues. You may also wish to include screenshots, examples and important design decisions you made in the description too.
- A collaborator will review your PR and accept it or make suggestions on what should be changed.

We appreciate your kind efforts in making Aurora a better software! :tada:
