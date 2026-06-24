import type { Problem, Submission } from "@/types"

export const problems: Problem[] = [
  {
    id: "prob-001",
    title: "Two Sum",
    slug: "two-sum",
    description:
      "Given an array of integers `nums` and an integer `target`, return indices of the two numbers such that they add up to `target`. You may assume that each input would have exactly one solution.",
    difficulty: "easy",
    tags: ["array", "hash-table"],
    constraints: ["2 <= nums.length <= 10^4", "-10^9 <= nums[i] <= 10^9"],
    examples: [
      {
        input: "nums = [2,7,11,15], target = 9",
        output: "[0,1]",
        explanation: "Because nums[0] + nums[1] == 9, we return [0, 1].",
      },
    ],
    testCases: [
      {
        id: "tc-001",
        input: "[2,7,11,15]\n9",
        expectedOutput: "[0,1]",
        isHidden: false,
      },
      {
        id: "tc-002",
        input: "[3,2,4]\n6",
        expectedOutput: "[1,2]",
        isHidden: false,
      },
    ],
    starterCode: {
      python:
        "class Solution:\n    def twoSum(self, nums: List[int], target: int) -> List[int]:\n        # Your code here",
      javascript: "function twoSum(nums, target) {\n    // Your code here\n}",
      cpp: "class Solution {\npublic:\n    vector<int> twoSum(vector<int>& nums, int target) {\n        // Your code here\n    }\n};",
      java: "class Solution {\n    public int[] twoSum(int[] nums, int target) {\n        // Your code here\n    }\n}",
    },
    timeLimit: 2000,
    memoryLimit: 256,
    acceptedCount: 1247,
    submissionCount: 3456,
  },
  {
    id: "prob-002",
    title: "Binary Search",
    slug: "binary-search",
    description:
      "Given an array of integers `nums` which is sorted in ascending order, and an integer `target`, write a function to search `target` in `nums`. If `target` exists, then return its index. Otherwise, return `-1`.",
    difficulty: "easy",
    tags: ["array", "binary-search"],
    constraints: [
      "1 <= nums.length <= 10^4",
      "All integers are sorted in ascending order.",
    ],
    examples: [{ input: "nums = [-1,0,3,5,9,12], target = 9", output: "4" }],
    testCases: [
      {
        id: "tc-003",
        input: "[-1,0,3,5,9,12]\n9",
        expectedOutput: "4",
        isHidden: false,
      },
    ],
    starterCode: {
      python:
        "class Solution:\n    def search(self, nums: List[int], target: int) -> int:\n        # Your code here",
      javascript: "function search(nums, target) {\n    // Your code here\n}",
      cpp: "class Solution {\npublic:\n    int search(vector<int>& nums, int target) {\n        // Your code here\n    }\n};",
      java: "class Solution {\n    public int search(int[] nums, int target) {\n        // Your code here\n    }\n}",
    },
    timeLimit: 1000,
    memoryLimit: 128,
    acceptedCount: 892,
    submissionCount: 2103,
  },
  {
    id: "prob-003",
    title: "Merge Sort Implementation",
    slug: "merge-sort",
    description:
      "Implement the merge sort algorithm to sort an array of integers in ascending order. The algorithm should have O(n log n) time complexity.",
    difficulty: "medium",
    tags: ["sorting", "divide-and-conquer", "recursion"],
    constraints: ["1 <= nums.length <= 10^5"],
    examples: [{ input: "nums = [5,2,3,1]", output: "[1,2,3,5]" }],
    testCases: [
      {
        id: "tc-004",
        input: "[5,2,3,1]",
        expectedOutput: "[1,2,3,5]",
        isHidden: false,
      },
    ],
    starterCode: {
      python:
        "class Solution:\n    def sortArray(self, nums: List[int]) -> List[int]:\n        # Your code here",
      javascript: "function sortArray(nums) {\n    // Your code here\n}",
      cpp: "class Solution {\npublic:\n    vector<int> sortArray(vector<int>& nums) {\n        // Your code here\n    }\n};",
      java: "class Solution {\n    public int[] sortArray(int[] nums) {\n        // Your code here\n    }\n}",
    },
    timeLimit: 3000,
    memoryLimit: 512,
    acceptedCount: 456,
    submissionCount: 1567,
  },
  {
    id: "prob-004",
    title: "Longest Palindromic Substring",
    slug: "longest-palindromic-substring",
    description:
      "Given a string `s`, return the longest palindromic substring in `s`. A palindrome is a string that reads the same backward as forward.",
    difficulty: "medium",
    tags: ["string", "dynamic-programming"],
    constraints: ["1 <= s.length <= 1000"],
    examples: [{ input: 's = "babad"', output: '"bab"' }],
    testCases: [
      { id: "tc-005", input: "babad", expectedOutput: "bab", isHidden: false },
    ],
    starterCode: {
      python:
        "class Solution:\n    def longestPalindrome(self, s: str) -> str:\n        # Your code here",
      javascript: "function longestPalindrome(s) {\n    // Your code here\n}",
      cpp: "class Solution {\npublic:\n    string longestPalindrome(string s) {\n        // Your code here\n    }\n};",
      java: "class Solution {\n    public String longestPalindrome(String s) {\n        // Your code here\n    }\n}",
    },
    timeLimit: 2000,
    memoryLimit: 256,
    acceptedCount: 623,
    submissionCount: 1892,
  },
  {
    id: "prob-005",
    title: "LRU Cache",
    slug: "lru-cache",
    description:
      "Design a data structure that follows the constraints of a Least Recently Used (LRU) cache. Implement get and put operations in O(1) time.",
    difficulty: "hard",
    tags: ["design", "hash-table", "linked-list"],
    constraints: ["1 <= capacity <= 3000"],
    examples: [
      {
        input: '["LRUCache", "put", "put", "get"]\n[[2], [1, 1], [2, 2], [1]]',
        output: "[null, null, null, 1]",
      },
    ],
    testCases: [
      {
        id: "tc-006",
        input: "2\nput 1 1\nput 2 2\nget 1",
        expectedOutput: "1",
        isHidden: false,
      },
    ],
    starterCode: {
      python:
        "class LRUCache:\n    def __init__(self, capacity: int):\n        # Your code here\n\n    def get(self, key: int) -> int:\n        # Your code here\n\n    def put(self, key: int, value: int) -> None:\n        # Your code here",
      javascript:
        "class LRUCache {\n    constructor(capacity) {\n        // Your code here\n    }\n\n    get(key) {\n        // Your code here\n    }\n\n    put(key, value) {\n        // Your code here\n    }\n}",
      cpp: "class LRUCache {\npublic:\n    LRUCache(int capacity) {\n        // Your code here\n    }\n\n    int get(int key) {\n        // Your code here\n    }\n\n    void put(int key, int value) {\n        // Your code here\n    }\n};",
      java: "class LRUCache {\n    public LRUCache(int capacity) {\n        // Your code here\n    }\n\n    public int get(int key) {\n        // Your code here\n    }\n\n    public void put(int key, int value) {\n        // Your code here\n    }\n}",
    },
    timeLimit: 2000,
    memoryLimit: 256,
    acceptedCount: 234,
    submissionCount: 987,
  },
]

export const submissions: Submission[] = [
  {
    id: "sub-001",
    userId: "stu-001",
    problemId: "prob-001",
    type: "code",
    content:
      "class Solution:\n    def twoSum(self, nums, target):\n        seen = {}\n        for i, num in enumerate(nums):\n            if target - num in seen:\n                return [seen[target - num], i]\n            seen[num] = i",
    language: "python",
    verdict: "accepted",
    score: 100,
    maxScore: 100,
    executionTime: 45,
    memoryUsed: 14,
    submittedAt: "2026-01-10T14:30:00",
    gradedAt: "2026-01-10T14:30:05",
  },
  {
    id: "sub-002",
    userId: "stu-002",
    problemId: "prob-001",
    type: "code",
    content:
      "function twoSum(nums, target) {\n    for (let i = 0; i < nums.length; i++) {\n        for (let j = i + 1; j < nums.length; j++) {\n            if (nums[i] + nums[j] === target) return [i, j];\n        }\n    }\n}",
    language: "javascript",
    verdict: "accepted",
    score: 100,
    maxScore: 100,
    executionTime: 102,
    memoryUsed: 42,
    submittedAt: "2026-01-11T09:15:00",
    gradedAt: "2026-01-11T09:15:03",
  },
  {
    id: "sub-003",
    userId: "stu-001",
    problemId: "prob-002",
    type: "code",
    content:
      "class Solution:\n    def search(self, nums, target):\n        left, right = 0, len(nums) - 1\n        while left <= right:\n            mid = (left + right) // 2\n            if nums[mid] == target: return mid\n            elif nums[mid] < target: left = mid + 1\n            else: right = mid - 1\n        return -1",
    language: "python",
    verdict: "accepted",
    score: 100,
    maxScore: 100,
    executionTime: 32,
    memoryUsed: 15,
    submittedAt: "2026-01-12T16:45:00",
    gradedAt: "2026-01-12T16:45:04",
  },
  {
    id: "sub-004",
    userId: "stu-003",
    problemId: "prob-003",
    type: "code",
    content: "def sortArray(nums):\n    return sorted(nums)",
    language: "python",
    verdict: "wrong-answer",
    score: 50,
    maxScore: 100,
    submittedAt: "2026-01-13T11:20:00",
    gradedAt: "2026-01-13T11:20:02",
  },
  {
    id: "sub-005",
    userId: "stu-004",
    problemId: "prob-005",
    type: "code",
    content:
      "class LRUCache:\n    def __init__(self, capacity):\n        self.cache = {}\n        self.capacity = capacity",
    language: "python",
    verdict: "runtime-error",
    submittedAt: "2026-01-14T08:30:00",
    gradedAt: "2026-01-14T08:30:01",
  },
]
