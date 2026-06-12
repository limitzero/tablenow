---
name: create-spec
description: >
  Create one more structured feature specifications with self-contained task files organized into
  parallel execution phases from the 'stories.md' file. Use this skill when the user says "create a spec", "plan this feature",
  "write up an implementation plan", "break this into tasks", or after any planning conversation
  where the user wants to capture decisions as actionable spec files. Also use when the user
  says "/create-spec" or wants to decompose a feature into work items that agents can implement
  independently. This skill produces local spec files under specs/{feature}/ — no GitHub integration.
argument-hint: <path-to-stories-document>
---

Input: `$ARGUMENTS`

# Create Feature Specification

Transform a planning conversation into a structured spec folder that enables parallel agent implementation. The spec breaks a feature into self-contained task files — each one detailed enough that a coder agent can pick it up cold and implement it without reading anything else.

The key insight: implementation plans that live in a single file are either too large for a context window or too shallow for independent execution. By splitting into one file per task with full context in each, we enable multiple agents to work in parallel while keeping each agent's context focused.

## When to Use

- After a planning conversation where requirements and technical details have been discussed
- When the user asks to create a spec, plan a feature, or break work into tasks
- When the user wants to prepare work for parallel agent implementation


## Instructions


### Step 1: Examine extracted features from product plan 

Examine the files found at the folder `.docs/features` for any markdown files in the directory, if there are not any files listed, prompt the user to run the skill `/create-specs` for creating the listing of features from an existing product plan and exit. 


### Step 2: Decompose the feature files

For each story found in `.docs/features/stories.md`:

  1. Create a directory named `.docs/features/{story-name}` if it does not exist, the `{story-name}` should be in kebab-case with no special characters

  2. Break the implementation into atomic tasks. Each task should:
    - Be completable in a single coding session by one agent
    - Have a clear, specific scope (one concern per task)
    - Produce working, testable code when complete
    - Not overlap in files modified with other tasks in the same wave 

  Think carefully about granularity. Too coarse and agents can't work in parallel. Too fine and the overhead of context-switching between tasks dominates. A good task typically creates or modifies 1-5 files around a single concern.

  Include a task for unit or integration tests as a first-pass for verification of the functionality where applicable. 

  4. Build a dependency graph 
    
      For each task, identify:
        - **What it depends on**: which tasks must complete before this one can start
        - **What depends on it**: which tasks are blocked until this one finishes

      Tasks with no dependencies form Phase 1. Tasks whose dependencies are all in Phase 1 form Phase 2. And so on. All tasks within a phase can execute in parallel.

      When assigning phases, verify that tasks within the same phase do not modify overlapping files. If two tasks in the same phase would touch the same file, move one to a later phase — parallel agents on the same branch cannot safely modify the same file.


  5. Create the spec folder with the atomized tasks

      Create the following structure at `.docs/features/{feature-name}/`:

      ```
      .docs/features/{feature-name}/
      ├── README.md
      ├── requirements.md
      ├── action-required.md
      ├── implementation-plan.md
      └── tasks/
          ├── task-01-{name}.md
          ├── task-02-{name}.md
          └── ...
      ```

      Read the templates in `references/` before writing each file:
      - `references/readme-template.md` — for the README (dependency graph, phase table, status tracking)
      - `references/task-template.md` — for each task file (self-contained with all context)
      - `references/requirements-template.md` — for the requirements document
      - `references/action-required-template.md` — for manual human steps

      Task files are numbered with zero-padded two-digit prefixes in topological order: Phase 1 tasks first, then Phase 2, etc. Within a phase, order is arbitrary but stable.


  6. Write self-contained task files

      This is the most important step. Each task file is the **only thing** a coder agent will read before implementing. It must contain everything the agent needs:

      - **Description**: what to build and why it matters in context
      - **Dependency context**: what prior tasks produce that this task needs (summarized in prose, not just filenames). The agent should not need to read other task files.
      - **Technical details**: CLI commands, code snippets, schemas, file paths, env vars, API endpoints — every implementation-specific detail from the planning conversation
      - **Files to create/modify**: explicit list with purpose for each
      - **Acceptance criteria**: specific, verifiable conditions

      Review each task file with fresh eyes: could an agent who has never seen the planning conversation implement this correctly using only this file? If not, add what's missing.

  7. Extract manual actions

      Identify any steps that require human action (account creation, API key setup, DNS configuration, environment variables, third-party service registration, etc.). Write these to `action-required.md` grouped by timing (Before/During/After implementation). If none exist, note "No manual steps required."


  8. Report to User


      After creating the spec, display:

      ```
      Feature specification created at .docs/features/{feature-name}

      Files created:
      - README.md (dependency graph, {N} phases, {T} tasks)
      - requirements.md
      - action-required.md
      - tasks/ ({T} task files)

      Phase breakdown:
      - Phase 1: {count} tasks (parallel) — {brief description}
      - Phase 2: {count} tasks (parallel) — {brief description}
      - ...

      Next steps:
      1. Review action-required.md for tasks you need to complete manually
      2. Review the requirements and task files
      3. Use /implement-feature to start parallel implementation
      ```

  9. Create listing of all tasks by feature and phase

      There should be a markdown file named `implementation-plan.md` that is created in the directory `.docs/features/{feature-name}` that lists all tasks separated by phases that a coder agent can look at and pick up work and mark as complete.

      Use this structure for `implementation-plan.md`:

      ```markdown
      # Implementation Plan: {Feature Name}

      ## Overview

      Brief summary of what will be built.

      ## Phase 1: {Phase Name}

      {Brief description of this phase's goal}

      ### Tasks

      - [ ] Task 1 description
      - [ ] Task 2 description (depends on Task 1)
      - [ ] Task 3 description [complex]
        - [ ] Sub-task 3a
        - [ ] Sub-task 3b

      ### Technical Details

      {Include CLI commands, code snippets, schemas, and other implementation specifics discussed during planning that are relevant to this phase's tasks.}

      ## Phase 2: {Phase Name}

      {Brief description}

      ### Tasks

      - [ ] Task 4 description (depends on Phase 1)
      - [ ] Task 5 description

      ### Technical Details

      {Technical details for Phase 2 tasks.}
      ```

  ### Critical Rules

  - Every task file must be fully self-contained — this is the entire point of the spec structure. A coder agent reading only that file must know exactly what to do.
  - Capture ALL technical details from the planning and feature specification documents. The spec is the single source of truth — CLI commands, schemas, code snippets, file paths, env vars, API endpoints. Anything not captured here is lost.
  - Tasks within the same wave must not modify overlapping files. Parallel agents on the same branch cannot safely touch the same files.
  - Keep tasks atomic — one concern per task. If a task has more than 5-7 files to modify, consider splitting it.
  - Do not create testing tasks unless the user explicitly asks for them.
  - Number task files in topological order (phase 1 first, then phase 2, etc.) for easy scanning.
