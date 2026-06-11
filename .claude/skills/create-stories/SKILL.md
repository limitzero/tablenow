---
name: create-stories
description: Generate user stories from the product plan (i.e. PRD)
argument-hint: <path-to-prd>
---

## Create Stories from PRD

Generate structured user stories from a product requirements document (PRD). 

Input: `$ARGUMENTS`

___


## Phase 1 : LOAD

Read the PRD documnent provided as input. If no path is given, look for:

1. `.docs\prd\prd.md` files
2. `prd.md` at the project root directory 
3. Ask the user which PRD to use for the action 


Extract
* User stories already defined or alluded to in the selected PRD. 
* Acceptance criteria from the success criteria and/or requirements
* Implementation phases and their associated deliverables
* Technical constraints and dependencies

___

## Phase 2 : ANALYZE

### Break down into user stories 

For each feature or requirement in the PRD:

1. Create a user story in the format:

```markdown
As a [user type], I want to [action], so that [benefit]
```

2. Define the acceptance criteria  (3-5 per story)

```markdown
Given [context], when [action], then [expected result]
```

3. Estimate the complexity of the story: Small/ Medium / Large

    - **Small**: Single file change, clear implementation 
    - **Medium**: Multiple file changes, some design decisions
    - **Large**: Cross-cutting concerns, architectural changes

4. Identify possible dependencies between stories

### Story Categories

Group stories by type:
    - **Feature**: New functionality
    - **Enhancement**: Improvement to exisiting functionality
    - **Bug**: Fix for known issues
    - **Technical**: Infrastructure, refactoring, tooling
    - **Spike**: Research or investigation needed


___

## Phase 3 : Structure

```markdown
## [STORY-ID] Story Title

**Type**: Feature | Enhancement | Bug | Technical | Spike
**Priority**: High | Medium | Low
**Complexity**: Small | Medium | Large
**Phase**: (from PRD implementation phase)
**Labels**: (relevant labels like `frontend`, `backend`, `database`, `api`)


### Description: 
As a [user type], I want to [action], so that [benefit].

### Acceptance Criteria:
- [] Given [context], when [action], then [result]
- [] Given [context], when [action], then [result]
- [] Given [context], when [action], then [result]


### Technical Notes:
- Key implementation details 
- Files likely to be changed
- Patterns to follow (from CLAUDE.md or other conventions)

### Dependencies:
- Blocked by: [other story identifiers]

```

### Ordering 

Order stories by:
1. Phase (from PRD implementation)
2. Dependencies (blocked stories come after their blockers)
3. Priority (High first within each phase)


---

## Phase 4: VALIDATE


Before output, verify:
- [ ] Every PRD requirement maps to at least one story
- [ ] No story is too large (break down if > 1 day of work)
- [ ] Acceptance criteria are testable and specific
- [ ] Dependencies form a valid DAG (no circular dependencies)
- [ ] Stories cover the full SDLC: types, validation, services, routes, UI, tests
- [ ] Each story can be independently reviewed and merged

---

## Phase 5: OUTPUT

Create the directory if it doesn't exist: `mkdir -p .docs/features`

There should be a listing of all phases and tasks that can be tracked for any subsequent agent to look and pick up 
what items should be 
Save the stories to `.docs/features/` directory as a markdown file.
