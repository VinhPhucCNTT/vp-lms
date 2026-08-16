import type {
  Question,
  QuestionBank,
  AssessmentQuestion,
  AssessmentAttempt,
  QuestionType,
  QuestionDifficulty,
} from "@/types";

// ── Question Banks ────────────────────────────────────────────────────────────

export const questionBanks: QuestionBank[] = [
  {
    id: "qb-001",
    name: "Algorithms & Complexity",
    description: "Questions on algorithm analysis, Big O notation, and complexity theory.",
    ownerId: "ins-001",
    sharedWithInstructorIds: ["ins-002"],
    sharedWithCourseIds: ["cs-101"],
    questionIds: ["q-001", "q-002", "q-003", "q-004", "q-005"],
    createdAt: "2025-11-01",
  },
  {
    id: "qb-002",
    name: "Sorting & Searching",
    description: "Questions about sorting algorithms, searching techniques, and their applications.",
    ownerId: "ins-001",
    sharedWithInstructorIds: [],
    sharedWithCourseIds: [],
    questionIds: ["q-006", "q-007", "q-008"],
    createdAt: "2025-11-05",
  },
  {
    id: "qb-003",
    name: "Data Structures Fundamentals",
    description: "Core data structures: arrays, linked lists, stacks, queues, trees.",
    ownerId: "ins-002",
    sharedWithInstructorIds: ["ins-001"],
    sharedWithCourseIds: [],
    questionIds: ["q-009", "q-010", "q-011"],
    createdAt: "2025-10-20",
  },
  {
    id: "qb-004",
    name: "Database Systems",
    description: "SQL, normalization, transactions, and database design principles.",
    ownerId: "ins-003",
    sharedWithInstructorIds: [],
    sharedWithCourseIds: ["cs-301"],
    questionIds: ["q-012", "q-013"],
    createdAt: "2025-11-10",
  },
];

// ── Questions ──────────────────────────────────────────────────────────────────

export const questions: Question[] = [
  {
    id: "q-001",
    bankId: "qb-001",
    type: "multiple-choice",
    title: "Big O of Binary Search",
    text: "What is the time complexity of binary search on a sorted array of n elements?",
    points: 5,
    difficulty: "easy",
    options: [
      { id: "opt-a", text: "O(1)", isCorrect: false },
      { id: "opt-b", text: "O(log n)", isCorrect: true },
      { id: "opt-c", text: "O(n)", isCorrect: false },
      { id: "opt-d", text: "O(n²)", isCorrect: false },
    ],
    explanation: "Binary search halves the search space at each step, giving O(log n).",
    tags: ["big-o", "binary-search"],
  },
  {
    id: "q-002",
    bankId: "qb-001",
    type: "multiple-select",
    title: "Which are O(n log n) algorithms?",
    text: "Select all algorithms that have an average time complexity of O(n log n).",
    points: 10,
    difficulty: "medium",
    options: [
      { id: "opt-a", text: "Merge Sort", isCorrect: true },
      { id: "opt-b", text: "Quick Sort (average)", isCorrect: true },
      { id: "opt-c", text: "Bubble Sort", isCorrect: false },
      { id: "opt-d", text: "Heap Sort", isCorrect: true },
      { id: "opt-e", text: "Insertion Sort", isCorrect: false },
    ],
    explanation: "Merge, Quick (average), and Heap Sort are all O(n log n). Bubble and Insertion Sort are O(n²).",
    tags: ["sorting", "big-o"],
  },
  {
    id: "q-003",
    bankId: "qb-001",
    type: "true-false",
    title: "Big Omega Lower Bound",
    text: "Big Omega (Ω) notation describes the upper bound of an algorithm's running time.",
    points: 3,
    difficulty: "easy",
    correctAnswer: "false",
    explanation: "Big Omega describes the lower bound, not the upper bound. Big O describes the upper bound.",
    tags: ["big-o", "theory"],
  },
  {
    id: "q-004",
    bankId: "qb-001",
    type: "short-answer",
    title: "Define Asymptotic Analysis",
    text: "In one sentence, define what asymptotic analysis studies.",
    points: 5,
    difficulty: "medium",
    acceptedAnswers: [
      "asymptotic analysis studies the behavior of algorithms as input size approaches infinity",
      "it studies algorithm efficiency as input size grows to infinity",
      "analysis of algorithm performance as input size becomes very large",
    ],
    explanation: "Asymptotic analysis studies the growth rate of algorithms as the input size approaches infinity.",
    tags: ["theory"],
  },
  {
    id: "q-005",
    bankId: "qb-001",
    type: "essay",
    title: "Compare Quick Sort and Merge Sort",
    text: "Compare and contrast Quick Sort and Merge Sort in terms of time complexity, space complexity, stability, and when you would choose one over the other.",
    points: 20,
    difficulty: "hard",
    explanation: "Both are O(n log n) average. Merge Sort is stable, uses O(n) extra space. Quick Sort is in-place but unstable, O(n²) worst case.",
    tags: ["sorting", "comparison"],
  },
  {
    id: "q-006",
    bankId: "qb-002",
    type: "multiple-choice",
    title: "QuickSort Worst Case",
    text: "What is the worst-case time complexity of QuickSort?",
    points: 5,
    difficulty: "medium",
    options: [
      { id: "opt-a", text: "O(n log n)", isCorrect: false },
      { id: "opt-b", text: "O(n²)", isCorrect: true },
      { id: "opt-c", text: "O(n)", isCorrect: false },
      { id: "opt-d", text: "O(log n)", isCorrect: false },
    ],
    explanation: "QuickSort degrades to O(n²) when the pivot is always the smallest or largest element.",
    tags: ["sorting", "quicksort"],
  },
  {
    id: "q-007",
    bankId: "qb-002",
    type: "programming",
    title: "Implement Binary Search",
    text: "Implement a binary search function that returns the index of the target element in a sorted array, or -1 if not found.",
    points: 15,
    difficulty: "easy",
    problemId: "prob-002",
    language: "python",
    tags: ["binary-search", "arrays"],
  },
  {
    id: "q-008",
    bankId: "qb-002",
    type: "multiple-select",
    title: "Stable Sorting Algorithms",
    text: "Which of the following sorting algorithms are stable?",
    points: 10,
    difficulty: "medium",
    options: [
      { id: "opt-a", text: "Merge Sort", isCorrect: true },
      { id: "opt-b", text: "Quick Sort", isCorrect: false },
      { id: "opt-c", text: "Insertion Sort", isCorrect: true },
      { id: "opt-d", text: "Heap Sort", isCorrect: false },
    ],
    explanation: "Merge Sort and Insertion Sort are stable. Quick Sort and Heap Sort are not stable in their standard implementations.",
    tags: ["sorting", "stability"],
  },
  {
    id: "q-009",
    bankId: "qb-003",
    type: "multiple-choice",
    title: "Stack vs Queue",
    text: "Which data structure follows LIFO (Last In, First Out) ordering?",
    points: 3,
    difficulty: "easy",
    options: [
      { id: "opt-a", text: "Queue", isCorrect: false },
      { id: "opt-b", text: "Stack", isCorrect: true },
      { id: "opt-c", text: "Linked List", isCorrect: false },
      { id: "opt-d", text: "Binary Tree", isCorrect: false },
    ],
    explanation: "A stack follows LIFO ordering — the last element pushed is the first one popped.",
    tags: ["stacks", "queues"],
  },
  {
    id: "q-010",
    bankId: "qb-003",
    type: "true-false",
    title: "Hash Table Average Lookup",
    text: "The average time complexity for lookup in a hash table is O(1).",
    points: 3,
    difficulty: "easy",
    correctAnswer: "true",
    explanation: "With a good hash function, average lookup is O(1). Worst case is O(n) with collisions.",
    tags: ["hash-tables"],
  },
  {
    id: "q-011",
    bankId: "qb-003",
    type: "short-answer",
    title: "Binary Tree Height",
    text: "What is the minimum height of a binary tree with n nodes? (Express in Big O notation)",
    points: 5,
    difficulty: "medium",
    acceptedAnswers: ["o(log n)", "log n"],
    explanation: "A balanced binary tree with n nodes has a minimum height of O(log n).",
    tags: ["trees", "big-o"],
  },
  {
    id: "q-012",
    bankId: "qb-004",
    type: "multiple-choice",
    title: "Normal Form",
    text: "Which normal form eliminates transitive dependencies?",
    points: 5,
    difficulty: "medium",
    options: [
      { id: "opt-a", text: "1NF", isCorrect: false },
      { id: "opt-b", text: "2NF", isCorrect: false },
      { id: "opt-c", text: "3NF", isCorrect: true },
      { id: "opt-d", text: "BCNF", isCorrect: false },
    ],
    explanation: "Third Normal Form (3NF) eliminates transitive dependencies between non-key attributes.",
    tags: ["normalization", "database-design"],
  },
  {
    id: "q-013",
    bankId: "qb-004",
    type: "essay",
    title: "ACID Properties",
    text: "Explain the four ACID properties of database transactions and why each is important.",
    points: 20,
    difficulty: "hard",
    explanation: "ACID: Atomicity, Consistency, Isolation, Durability — fundamental to reliable transaction processing.",
    tags: ["transactions", "acid"],
  },
];

// ── Assessment-Question links ─────────────────────────────────────────────────

export const assessmentQuestions: AssessmentQuestion[] = [
  { id: "aq-001", assessmentId: "asm-001", questionId: "q-001", order: 1, points: 5 },
  { id: "aq-002", assessmentId: "asm-001", questionId: "q-003", order: 2, points: 3 },
  { id: "aq-003", assessmentId: "asm-001", questionId: "q-004", order: 3, points: 5 },
  { id: "aq-004", assessmentId: "asm-001", questionId: "q-009", order: 4, points: 3 },
  { id: "aq-005", assessmentId: "asm-002", questionId: "q-002", order: 1, points: 10 },
  { id: "aq-006", assessmentId: "asm-002", questionId: "q-006", order: 2, points: 5 },
  { id: "aq-007", assessmentId: "asm-002", questionId: "q-008", order: 3, points: 10 },
  { id: "aq-008", assessmentId: "asm-002", questionId: "q-007", order: 4, points: 15 },
  { id: "aq-009", assessmentId: "asm-002", questionId: "q-005", order: 5, points: 20 },
];

// ── Assessment Attempts ───────────────────────────────────────────────────────

export const assessmentAttempts: AssessmentAttempt[] = [
  {
    id: "att-001",
    assessmentId: "asm-001",
    studentId: "stu-001",
    attemptNumber: 1,
    status: "graded",
    score: 14,
    maxScore: 16,
    startedAt: "2026-01-15T10:00:00",
    submittedAt: "2026-01-15T10:25:00",
    timeSpent: 25,
    answers: [
      { questionId: "q-001", value: "opt-b", score: 5, graded: true },
      { questionId: "q-003", value: "false", score: 3, graded: true },
      { questionId: "q-004", value: "asymptotic analysis studies the behavior of algorithms as input size approaches infinity", score: 3, graded: true, feedback: "Good, but could be more precise about 'growth rate'." },
      { questionId: "q-009", value: "opt-b", score: 3, graded: true },
    ],
  },
  {
    id: "att-002",
    assessmentId: "asm-001",
    studentId: "stu-002",
    attemptNumber: 1,
    status: "graded",
    score: 16,
    maxScore: 16,
    startedAt: "2026-01-15T11:00:00",
    submittedAt: "2026-01-15T11:20:00",
    timeSpent: 20,
    answers: [
      { questionId: "q-001", value: "opt-b", score: 5, graded: true },
      { questionId: "q-003", value: "false", score: 3, graded: true },
      { questionId: "q-004", value: "it studies algorithm performance as input size grows to infinity", score: 5, graded: true },
      { questionId: "q-009", value: "opt-b", score: 3, graded: true },
    ],
  },
  {
    id: "att-003",
    assessmentId: "asm-002",
    studentId: "stu-001",
    attemptNumber: 1,
    status: "submitted",
    score: null,
    maxScore: 60,
    startedAt: "2026-02-10T14:00:00",
    submittedAt: "2026-02-10T14:55:00",
    timeSpent: 55,
    answers: [
      { questionId: "q-002", value: ["opt-a", "opt-b", "opt-d"], score: 10, graded: true },
      { questionId: "q-006", value: "opt-b", score: 5, graded: true },
      { questionId: "q-008", value: ["opt-a", "opt-c"], score: 10, graded: true },
      { questionId: "q-007", value: "class Solution:\n    def search(self, nums, target):\n        left, right = 0, len(nums) - 1\n        while left <= right:\n            mid = (left + right) // 2\n            if nums[mid] == target: return mid\n            elif nums[mid] < target: left = mid + 1\n            else: right = mid - 1\n        return -1", score: 15, graded: true },
      { questionId: "q-005", value: "Merge Sort is stable and uses O(n) extra space, while Quick Sort is in-place but unstable. Merge Sort guarantees O(n log n) in all cases, but Quick Sort has O(n²) worst case. I would choose Merge Sort when stability is required or when working with linked lists, and Quick Sort when memory is constrained and average performance matters more.", score: null, graded: false },
    ],
  },
  {
    id: "att-004",
    assessmentId: "asm-002",
    studentId: "stu-002",
    attemptNumber: 1,
    status: "submitted",
    score: null,
    maxScore: 60,
    startedAt: "2026-02-10T15:00:00",
    submittedAt: "2026-02-10T15:48:00",
    timeSpent: 48,
    answers: [
      { questionId: "q-002", value: ["opt-a", "opt-b", "opt-c"], score: 7, graded: true },
      { questionId: "q-006", value: "opt-b", score: 5, graded: true },
      { questionId: "q-008", value: ["opt-a", "opt-c"], score: 10, graded: true },
      { questionId: "q-007", value: "class Solution:\n    def search(self, nums, target):\n        return nums.index(target) if target in nums else -1", score: 10, graded: true, feedback: "Using .index() is O(n), not binary search. Implement the actual algorithm." },
      { questionId: "q-005", value: "Both are efficient sorting algorithms. Merge sort divides and merges. Quick sort uses pivots.", score: null, graded: false },
    ],
  },
];

// ── Helper functions ──────────────────────────────────────────────────────────

export function getQuestionsByBank(bankId: string): Question[] {
  return questions.filter((q) => q.bankId === bankId);
}

export function getQuestionsByAssessment(assessmentId: string): Question[] {
  return assessmentQuestions
    .filter((aq) => aq.assessmentId === assessmentId)
    .sort((a, b) => a.order - b.order)
    .map((aq) => questions.find((q) => q.id === aq.questionId))
    .filter((q): q is Question => q !== undefined);
}

export function getAssessmentLinks(assessmentId: string): AssessmentQuestion[] {
  return assessmentQuestions
    .filter((aq) => aq.assessmentId === assessmentId)
    .sort((a, b) => a.order - b.order);
}

export function getAttemptsByAssessment(assessmentId: string): AssessmentAttempt[] {
  return assessmentAttempts.filter((a) => a.assessmentId === assessmentId);
}

export function getAttemptsByStudent(studentId: string, assessmentId: string): AssessmentAttempt[] {
  return assessmentAttempts.filter(
    (a) => a.studentId === studentId && a.assessmentId === assessmentId
  );
}

export function getAssessmentTotalPoints(assessmentId: string): number {
  return getAssessmentLinks(assessmentId).reduce((sum, aq) => sum + aq.points, 0);
}

export function getQuestionById(questionId: string): Question | undefined {
  return questions.find((q) => q.id === questionId);
}

export function getBankById(bankId: string): QuestionBank | undefined {
  return questionBanks.find((b) => b.id === bankId);
}

export function getBanksByOwner(ownerId: string): QuestionBank[] {
  return questionBanks.filter((b) => b.ownerId === ownerId);
}

export function getSharedBanks(instructorId: string): QuestionBank[] {
  return questionBanks.filter((b) => b.sharedWithInstructorIds.includes(instructorId));
}

export const questionTypeLabels: Record<QuestionType, string> = {
  "multiple-choice": "Multiple Choice",
  "multiple-select": "Multiple Select",
  "true-false": "True / False",
  "short-answer": "Short Answer",
  "essay": "Essay",
  "programming": "Programming",
};

export const questionTypeIcons: Record<QuestionType, string> = {
  "multiple-choice": "CircleDotIcon",
  "multiple-select": "ListChecksIcon",
  "true-false": "ToggleLeftIcon",
  "short-answer": "TextIcon",
  "essay": "AlignLeftIcon",
  "programming": "CodeIcon",
};

export const difficultyColors: Record<QuestionDifficulty, string> = {
  easy: "bg-success/10 text-success border-success/20",
  medium: "bg-warning/10 text-warning-foreground border-warning/20",
  hard: "bg-destructive/10 text-destructive border-destructive/20",
};
