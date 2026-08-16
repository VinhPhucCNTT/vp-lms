import * as React from "react";
import { cn } from "@/lib/utils";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

export interface NavigatorQuestionState {
  questionId: string;
  answered: boolean;
  flagged: boolean;
}

interface QuestionNavigatorProps {
  states: NavigatorQuestionState[];
  currentIndex: number;
  onJump: (index: number) => void;
}

export function QuestionNavigator({ states, currentIndex, onJump }: QuestionNavigatorProps) {
  return (
    <TooltipProvider delayDuration={200}>
      <div className="grid grid-cols-5 gap-2">
        {states.map((state, i) => {
          const isCurrent = i === currentIndex;
          return (
            <Tooltip key={state.questionId}>
              <TooltipTrigger asChild>
                <button
                  onClick={() => onJump(i)}
                  className={cn(
                    "size-9 rounded-md text-xs font-medium transition-all flex items-center justify-center border",
                    isCurrent && "ring-2 ring-ring ring-offset-1",
                    state.answered && !state.flagged && "bg-primary text-primary-foreground border-primary",
                    !state.answered && !state.flagged && "bg-muted text-muted-foreground border-border",
                    state.flagged && "bg-warning text-warning-foreground border-warning"
                  )}
                >
                  {i + 1}
                </button>
              </TooltipTrigger>
              <TooltipContent side="top" className="text-xs">
                <p>Question {i + 1}</p>
                <p className="text-muted-foreground">
                  {state.flagged ? "Flagged" : state.answered ? "Answered" : "Unanswered"}
                </p>
              </TooltipContent>
            </Tooltip>
          );
        })}
      </div>
    </TooltipProvider>
  );
}
