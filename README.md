<h1>BrianBird_CS295N_Labs_W26</h1>

Demo code for the CS 295N ASP.NET MVC class at Lane Community College

<h2>Contents</h2>

[TOC]

## Requirements

### Work-Flow

1. Student logs into the web site.
2. Student A enters info about code that is ready for review:
   - Class CRN ("323312")
   - Assignment name and or number ("Lab 1")
   - Assignment version ("A")
   - Link to code (GitHub repo URL)
3. Stiudent B etners info about code ready for review
4. The system checks to see if student A and B are in different assignment groups. 
   - If they are, it emails them to let them know they have a match and can exchange code reviews.
   - If not, the system waits for another student to enter that they are ready and then checks again.

### Features

#### Essential

- Student enters info about code ready for review
- The system emails students when there is a match for students to exchange code reviews.

#### High Proiroity

- Admin can enter class CRNs, assignment names, and assignment version letters
  - These become options students can select when entering code info.
- Admins can see a list of student code entries
- Students can see their code entry and any matches.

#### Medium Priority

- Admins can upload lab instructions.
  - Students can download lab instructions.
- Admins can upload code review forms
  - Students can download code review forms
- Students can upload completed code review forms
  - Student who's code was reviewed can download the completed review
  - Admin can download completed reviews.
- Students can upload code files (zip) instead of a link.
  - Students who are matched can download code files.
  - Admins can download code files.





#### Assumptions and Constraints

- Students are registered on the web site.
  - Name
  - Email address
- Students will upload a link to a GitHub repo
  - In the future, file uploads could also be used.
- Students will specifiy which assignment and version of an assignment their code is for.
- Students will provide a way for the reviewer to communicate with them (email).

