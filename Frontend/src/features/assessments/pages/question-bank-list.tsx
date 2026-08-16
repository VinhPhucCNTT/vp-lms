import * as React from "react";
import { Link } from "react-router-dom";
import {
  PlusCircleIcon,
  SearchIcon,
  LibraryIcon,
  Share2Icon,
  LockIcon,
  ChevronRightIcon,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { PageHeader } from "@/shared/components/page-header";
import { questionBanks, getQuestionsByBank, getBanksByOwner, getSharedBanks } from "@/shared/data/question-bank";
import { instructors } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";
import { cn } from "@/lib/utils";

export function QuestionBankList() {
  const { user } = useAuth();
  const currentInstructor = instructors.find((i) => i.id === user?.id) ?? instructors[0];
  const [search, setSearch] = React.useState("");
  const [tab, setTab] = React.useState<"my" | "shared" | "all">("my");

  const myBanks = getBanksByOwner(currentInstructor.id);
  const sharedBanks = getSharedBanks(currentInstructor.id);

  const activeBanks = React.useMemo(() => {
    let banks: typeof questionBanks = [];
    if (tab === "my") banks = myBanks;
    else if (tab === "shared") banks = sharedBanks;
    else banks = questionBanks;

    if (search) {
      const lower = search.toLowerCase();
      banks = banks.filter(
        (b) =>
          b.name.toLowerCase().includes(lower) ||
          b.description?.toLowerCase().includes(lower)
      );
    }
    return banks;
  }, [tab, search, myBanks, sharedBanks]);

  return (
    <div className="space-y-6">
      <PageHeader
        title="Question Banks"
        description="Reusable question collections you can share and use across assessments"
        breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "Question Banks" }]}
        actions={
          <Button>
            <PlusCircleIcon className="size-4 mr-2" />New Bank
          </Button>
        }
      />

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <LibraryIcon className="size-4" />My Banks
            </CardTitle>
          </CardHeader>
          <CardContent><p className="text-2xl font-bold">{myBanks.length}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Share2Icon className="size-4" />Shared With Me
            </CardTitle>
          </CardHeader>
          <CardContent><p className="text-2xl font-bold">{sharedBanks.length}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Total Questions</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">
              {myBanks.reduce((s, b) => s + b.questionIds.length, 0) +
                sharedBanks.reduce((s, b) => s + b.questionIds.length, 0)}
            </p>
          </CardContent>
        </Card>
      </div>

      <div className="flex items-center justify-between gap-4">
        <div className="flex gap-1 rounded-lg border p-1">
          {([
            { key: "my", label: "My Banks" },
            { key: "shared", label: "Shared With Me" },
            { key: "all", label: "All Banks" },
          ] as const).map((t) => (
            <button
              key={t.key}
              onClick={() => setTab(t.key)}
              className={cn(
                "rounded-md px-3 py-1.5 text-sm font-medium transition-colors",
                tab === t.key ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:text-foreground"
              )}
            >
              {t.label}
            </button>
          ))}
        </div>
        <div className="relative flex-1 max-w-md">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input placeholder="Search banks..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {activeBanks.map((bank) => {
          const bankQuestions = getQuestionsByBank(bank.id);
          const owner = instructors.find((i) => i.id === bank.ownerId);
          const isOwner = bank.ownerId === currentInstructor.id;
          return (
            <Card key={bank.id} className="group hover:border-primary/40 transition-colors">
              <Link to={`/instructor/question-banks/${bank.id}`}>
                <CardContent className="p-5 space-y-3">
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-2">
                      <div className={cn(
                        "flex size-9 items-center justify-center rounded-lg",
                        isOwner ? "bg-primary/10 text-primary" : "bg-muted text-muted-foreground"
                      )}>
                        {isOwner ? <LibraryIcon className="size-4" /> : <Share2Icon className="size-4" />}
                      </div>
                      <div>
                        <h3 className="text-sm font-semibold group-hover:text-primary transition-colors">{bank.name}</h3>
                        <p className="text-xs text-muted-foreground">
                          {isOwner ? "Owned" : `Shared by ${owner?.firstName} ${owner?.lastName}`}
                        </p>
                      </div>
                    </div>
                    {!isOwner && <LockIcon className="size-3.5 text-muted-foreground" />}
                  </div>
                  <p className="text-xs text-muted-foreground line-clamp-2">{bank.description}</p>
                  <div className="flex items-center justify-between pt-1">
                    <Badge variant="outline">{bankQuestions.length} questions</Badge>
                    <ChevronRightIcon className="size-4 text-muted-foreground group-hover:text-primary transition-colors" />
                  </div>
                </CardContent>
              </Link>
            </Card>
          );
        })}
        {activeBanks.length === 0 && (
          <div className="col-span-full py-12 text-center">
            <LibraryIcon className="size-10 text-muted-foreground mx-auto mb-2" />
            <p className="text-sm text-muted-foreground">No question banks found.</p>
          </div>
        )}
      </div>
    </div>
  );
}
