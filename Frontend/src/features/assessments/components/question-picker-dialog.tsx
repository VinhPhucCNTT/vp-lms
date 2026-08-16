import * as React from "react";
import { SearchIcon, PlusIcon, CheckIcon } from "lucide-react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Checkbox } from "@/components/ui/checkbox";
import { cn } from "@/lib/utils";
import type { Question } from "@/types";
import { questionBanks, questions, questionTypeLabels, getQuestionsByBank } from "@/shared/data/question-bank";
import { questionTypeIconMap } from "./question-answer-renderer";

interface QuestionPickerDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onAdd: (questionIds: string[]) => void;
  excludeIds?: string[];
}

export function QuestionPickerDialog({ open, onOpenChange, onAdd, excludeIds = [] }: QuestionPickerDialogProps) {
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const [search, setSearch] = React.useState("");
  const [activeBank, setActiveBank] = React.useState<string | null>(null);

  React.useEffect(() => {
    if (open) {
      setSelected(new Set());
      setSearch("");
      setActiveBank(null);
    }
  }, [open]);

  const availableQuestions = React.useMemo(() => {
    let qs = questions.filter((q) => !excludeIds.includes(q.id));
    if (activeBank) qs = qs.filter((q) => q.bankId === activeBank);
    if (search) {
      const lower = search.toLowerCase();
      qs = qs.filter(
        (q) =>
          q.title.toLowerCase().includes(lower) ||
          q.text.toLowerCase().includes(lower) ||
          q.tags?.some((t) => t.includes(lower))
      );
    }
    return qs;
  }, [activeBank, search, excludeIds]);

  const toggle = (id: string) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleAdd = () => {
    onAdd(Array.from(selected));
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl max-h-[80vh]">
        <DialogHeader>
          <DialogTitle>Add Questions from Bank</DialogTitle>
        </DialogHeader>

        <div className="flex gap-3 items-center">
          <div className="relative flex-1">
            <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
            <Input
              placeholder="Search questions..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="pl-9"
            />
          </div>
          <select
            value={activeBank ?? ""}
            onChange={(e) => setActiveBank(e.target.value || null)}
            className="h-9 rounded-md border border-input bg-background px-3 text-sm"
          >
            <option value="">All Banks</option>
            {questionBanks.map((b) => (
              <option key={b.id} value={b.id}>
                {b.name}
              </option>
            ))}
          </select>
        </div>

        <ScrollArea className="h-[400px] rounded-lg border">
          <div className="divide-y">
            {availableQuestions.length === 0 && (
              <div className="py-16 text-center text-sm text-muted-foreground">
                No questions found.
              </div>
            )}
            {availableQuestions.map((q) => {
              const Icon = questionTypeIconMap[q.type];
              const isSelected = selected.has(q.id);
              const bank = questionBanks.find((b) => b.id === q.bankId);
              return (
                <div
                  key={q.id}
                  onClick={() => toggle(q.id)}
                  className={cn(
                    "flex items-start gap-3 p-3 cursor-pointer transition-colors",
                    isSelected ? "bg-primary/5" : "hover:bg-muted/50"
                  )}
                >
                  <Checkbox checked={isSelected} className="mt-1" />
                  <div className="flex-1 min-w-0 space-y-1">
                    <div className="flex items-center gap-2 flex-wrap">
                      <Icon className="size-3.5 text-muted-foreground" />
                      <span className="text-sm font-medium truncate">{q.title}</span>
                      <Badge variant="outline" className="text-xs">{q.points} pts</Badge>
                      <Badge
                        variant="outline"
                        className={cn(
                          "text-xs",
                          q.difficulty === "easy" && "border-success/20 text-success",
                          q.difficulty === "medium" && "border-warning/20 text-warning-foreground",
                          q.difficulty === "hard" && "border-destructive/20 text-destructive"
                        )}
                      >
                        {q.difficulty}
                      </Badge>
                    </div>
                    <p className="text-xs text-muted-foreground line-clamp-2">{q.text}</p>
                    <div className="flex items-center gap-2 text-xs text-muted-foreground">
                      <span>{questionTypeLabels[q.type]}</span>
                      {bank && (
                        <>
                          <span>·</span>
                          <span>{bank.name}</span>
                        </>
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </ScrollArea>

        <DialogFooter className="justify-between">
          <span className="text-sm text-muted-foreground">
            {selected.size} selected
          </span>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button onClick={handleAdd} disabled={selected.size === 0}>
              <PlusIcon className="size-4 mr-1" />
              Add {selected.size > 0 && `(${selected.size})`}
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
