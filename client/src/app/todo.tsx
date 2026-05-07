import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { Check, X } from "lucide-react";
import { useState } from "react";

type Todo = {
  id: number;
  text: string;
  completed: boolean;
};

export default function Todo() {
  const [todos, setTodos] = useState<Todo[]>([
    { id: 1, text: "11111", completed: true },
    { id: 2, text: "22222", completed: false },
    { id: 3, text: "33333", completed: false },
    { id: 4, text: "44444", completed: false },
  ]);

  const [input, setInput] = useState("");

  const addTodo = () => {
    const newTodo: Todo = {
      id: todos.length + 1,
      text: input,
      completed: false,
    };
    setTodos((prevTodos) => [...prevTodos, newTodo]);
  };

  return (
    <>
      <div className="gap-4">
        <Input
          placeholder="название"
          onChange={(event) => setInput(event.target.value)}
        />
        <Button onClick={addTodo} variant="outline">
          Добавить задачу
        </Button>
      </div>
      <div className="grid gap-4 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
        {todos.map((todo) => (
          <Card
            key={todo.id}
            className="overflow-hidden transition-all hover:shadow-md"
          >
            <CardContent className="p-6">
              <div className="flex items-start justify-between gap-4">
                <div className="space-y-1">
                  <span className="text-xs font-mono text-muted-foreground">
                    ID-{todo.id}
                  </span>
                  <p
                    className={cn(
                      "text-sm font-medium leading-none",
                      todo.completed && "text-muted-foreground line-through",
                    )}
                  >
                    {todo.text}
                  </p>
                </div>

                <div className="shrink-0">
                  {todo.completed ? (
                    <Badge
                      variant="secondary"
                      className="bg-green-100 text-green-700 hover:bg-green-100 border-transparent"
                    >
                      <Check className="mr-1 h-3 w-3" />
                      Готово
                    </Badge>
                  ) : (
                    <Badge variant="outline" className="text-muted-foreground">
                      <X className="mr-1 h-3 w-3" />В работе
                    </Badge>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </>
  );
}
