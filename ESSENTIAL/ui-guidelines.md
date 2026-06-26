# UI Guidelines — Preferential Rules of Origin Calculation System

> **Version:** 1.0  
> **Scope:** Frontend — React 19, TypeScript, shadcn/ui, Tailwind CSS 4  
> **Approved UI Library:** shadcn/ui ONLY. Material UI, Ant Design, and Chakra UI are NOT permitted.

---

## Table of Contents

1. [Design Philosophy](#1-design-philosophy)
2. [Color Palette](#2-color-palette)
3. [Dark Mode](#3-dark-mode)
4. [Typography](#4-typography)
5. [Spacing System](#5-spacing-system)
6. [Icons](#6-icons)
7. [Buttons](#7-buttons)
8. [Forms](#8-forms)
9. [Dialogs](#9-dialogs)
10. [Tables and DataTable](#10-tables-and-datatable)
11. [Searchable Dropdowns](#11-searchable-dropdowns)
12. [Multi-Select Controls](#12-multi-select-controls)
13. [Cards](#13-cards)
14. [Navigation](#14-navigation)
15. [Dashboard Layout](#15-dashboard-layout)
16. [Origin Calculator Stepper / Wizard](#16-origin-calculator-stepper--wizard)
17. [Rule Visualization Component](#17-rule-visualization-component)
18. [Rule Builder UI](#18-rule-builder-ui)
19. [Responsive Rules](#19-responsive-rules)
20. [Accessibility](#20-accessibility)
21. [Loading States](#21-loading-states)
22. [Notifications](#22-notifications)
23. [Error Screens](#23-error-screens)
24. [Empty States](#24-empty-states)
25. [Data Visualization](#25-data-visualization)
26. [Animations](#26-animations)
27. [Component Naming Conventions](#27-component-naming-conventions)
28. [Folder Structure](#28-folder-structure)
29. [Design Tokens](#29-design-tokens)

---

## 1. Design Philosophy

The Preferential Rules of Origin system is an enterprise regulatory tool used by trade compliance officers, customs analysts, and supply-chain teams. The UI must prioritize:

### Clarity Over Decoration
- Every element must serve a functional purpose. Decorative graphics, gradient backgrounds, and drop-shadow stacking are not used.
- Data density is acceptable when it serves the user. Tables may show up to 12 columns on desktop; do not artificially paginate data that fits.
- White space is used deliberately to separate logical groups, not to fill space.

### Accessibility First
- WCAG AA compliance is the minimum bar. All interactive elements must be keyboard-navigable.
- Colour alone never conveys state — always pair colour with an icon or text label (e.g. "Originating" label alongside the green badge).
- Focus indicators must be visible at all times. Do not suppress `:focus-visible` outlines.

### Enterprise SaaS Aesthetic
- The visual language is derived from shadcn/ui defaults: neutral greys, a single primary blue, and semantic colours (success, warning, destructive) used sparingly.
- Avoid trendy design patterns (glassmorphism, floating blobs, neon colours). The palette is professional, slightly conservative, and appropriate for regulatory software.
- Consistency across all modules (Dashboard, Countries, Trade Agreements, HS Codes, Rules, Products, Materials, Origin Calculator, Reports, Settings) is mandatory. Do not invent per-module visual languages.

### Performance Perception
- Skeleton loaders ship for every data-fetching component. Users must never see blank space while data loads.
- Optimistic updates are applied for low-risk mutations (status toggles, simple edits).
- Heavy pages (large tables, complex visualisations) use React Suspense boundaries so the chrome (sidebar, topbar) renders immediately.

---

## 2. Color Palette

### CSS Custom Properties

All colours are defined as HSL triplets in `frontend/src/index.css`. Never reference raw hex or RGB values in component code — always use the CSS variable.

```css
/* frontend/src/index.css */
:root {
  --background:             0 0% 100%;         /* #ffffff  — page background          */
  --foreground:             222.2 84% 4.9%;    /* #0f172a  — primary text             */

  --primary:                221.2 83.2% 53.3%; /* #3b82f6  — blue-600, CTAs           */
  --primary-foreground:     210 40% 98%;       /* #f8fafc  — text on primary bg       */

  --secondary:              210 40% 96.1%;     /* #f1f5f9  — secondary surfaces       */
  --secondary-foreground:   222.2 47.4% 11.2%; /* #1e293b  — text on secondary bg     */

  --muted:                  210 40% 96.1%;     /* #f1f5f9  — muted backgrounds        */
  --muted-foreground:       215.4 16.3% 46.9%; /* #64748b  — placeholder, captions    */

  --accent:                 210 40% 96.1%;     /* #f1f5f9  — hover highlight          */
  --accent-foreground:      222.2 47.4% 11.2%; /* #1e293b                             */

  --destructive:            0 84.2% 60.2%;     /* #ef4444  — red-500, errors          */
  --destructive-foreground: 210 40% 98%;       /* #f8fafc                             */

  --border:                 214.3 31.8% 91.4%; /* #e2e8f0  — borders, dividers        */
  --input:                  214.3 31.8% 91.4%; /* #e2e8f0  — input borders            */
  --ring:                   221.2 83.2% 53.3%; /* #3b82f6  — focus rings              */

  --success:                142.1 76.2% 36.3%; /* #16a34a  — green-600, passed rules  */
  --warning:                45.4 93.4% 47.5%;  /* #f59e0b  — amber-500, warnings      */

  --radius:                 0.5rem;            /* 8px — base border radius            */
}
```

### Colour Reference Table

| Token                  | HSL                    | Hex       | Tailwind Equivalent | Usage                                |
|------------------------|------------------------|-----------|---------------------|--------------------------------------|
| `--background`         | 0 0% 100%              | `#ffffff` | `bg-white`          | Page / panel background              |
| `--foreground`         | 222.2 84% 4.9%         | `#0f172a` | `text-slate-950`    | Body text, headings                  |
| `--primary`            | 221.2 83.2% 53.3%      | `#3b82f6` | `bg-blue-500`       | Primary buttons, active nav, links   |
| `--primary-foreground` | 210 40% 98%            | `#f8fafc` | `text-slate-50`     | Text on primary backgrounds          |
| `--secondary`          | 210 40% 96.1%          | `#f1f5f9` | `bg-slate-100`      | Secondary button backgrounds         |
| `--muted`              | 210 40% 96.1%          | `#f1f5f9` | `bg-slate-100`      | Table striping, code blocks          |
| `--muted-foreground`   | 215.4 16.3% 46.9%      | `#64748b` | `text-slate-500`    | Placeholder text, captions, metadata |
| `--destructive`        | 0 84.2% 60.2%          | `#ef4444` | `text-red-500`      | Error messages, delete actions       |
| `--border`             | 214.3 31.8% 91.4%      | `#e2e8f0` | `border-slate-200`  | All borders                          |
| `--success`            | 142.1 76.2% 36.3%      | `#16a34a` | `text-green-600`    | Passed rule badges, success toasts   |
| `--warning`            | 45.4 93.4% 47.5%       | `#f59e0b` | `text-amber-500`    | Warning toasts, pending states       |

### Semantic Colour Usage Rules

- **Do not** use raw Tailwind colour classes (e.g., `text-blue-500`) for semantic UI elements. Use the CSS variable utility classes provided by shadcn/ui (e.g., `text-primary`, `bg-destructive`).
- **Do** use raw Tailwind colour classes only inside chart colour arrays (Recharts) and illustration SVGs.
- The `--success` and `--warning` tokens are custom additions. Add them to `tailwind.config.ts` as described in Section 29.

---

## 3. Dark Mode

### Strategy: Class-Based Switching

Dark mode uses the `class` strategy (not `media` strategy) so the user's in-app preference persists independently of the OS setting.

```typescript
// frontend/src/main.tsx or root provider
import { ThemeProvider } from 'next-themes'

<ThemeProvider attribute="class" defaultTheme="system" enableSystem>
  <App />
</ThemeProvider>
```

### CSS Variable Overrides

Define dark-mode variable overrides in `frontend/src/index.css` under the `.dark` selector:

```css
.dark {
  --background:           222.2 84% 4.9%;
  --foreground:           210 40% 98%;

  --primary:              217.2 91.2% 59.8%;
  --primary-foreground:   222.2 47.4% 11.2%;

  --secondary:            217.2 32.6% 17.5%;
  --secondary-foreground: 210 40% 98%;

  --muted:                217.2 32.6% 17.5%;
  --muted-foreground:     215 20.2% 65.1%;

  --accent:               217.2 32.6% 17.5%;
  --accent-foreground:    210 40% 98%;

  --destructive:          0 62.8% 30.6%;
  --destructive-foreground: 210 40% 98%;

  --border:               217.2 32.6% 17.5%;
  --input:                217.2 32.6% 17.5%;
  --ring:                 224.3 76.3% 48%;

  --success:              142.1 70.6% 45.3%;
  --warning:              45.4 93.4% 58%;
}
```

### Rules

- **Never hardcode colours.** Every colour in a component must reference a CSS variable or a Tailwind utility that maps to one (e.g., `bg-background`, `text-foreground`, `border-border`).
- **Never** use `dark:bg-gray-900` in isolation — always start from the semantic token.
- Images and chart fill colours are the only exception; they use explicit Tailwind or hex values.

---

## 4. Typography

### Font Stack

```css
/* frontend/src/index.css */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

body {
  font-family: 'Inter', ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont,
               'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
}
```

Set Inter as the default sans font in `tailwind.config.ts`:

```typescript
theme: {
  extend: {
    fontFamily: {
      sans: ['Inter', 'ui-sans-serif', 'system-ui'],
    },
  },
}
```

### Type Scale

| Class        | Size    | Line Height | Usage                                    |
|--------------|---------|-------------|------------------------------------------|
| `text-xs`    | 12px    | 16px        | Table cell metadata, timestamps, badges  |
| `text-sm`    | 14px    | 20px        | Form labels, helper text, nav items      |
| `text-base`  | 16px    | 24px        | Body text, table cell content            |
| `text-lg`    | 18px    | 28px        | Card titles, section sub-headings        |
| `text-xl`    | 20px    | 28px        | Page sub-headings                        |
| `text-2xl`   | 24px    | 32px        | Page headings (h1 within content area)   |
| `text-3xl`   | 30px    | 36px        | Dashboard stat card numbers              |
| `text-4xl`   | 36px    | 40px        | Error page codes (404, 500)              |

### Heading Hierarchy

```tsx
// Page title — rendered once per page
<h1 className="text-2xl font-semibold tracking-tight text-foreground">
  Trade Agreements
</h1>

// Section heading inside a page
<h2 className="text-lg font-semibold text-foreground">
  Active Agreements
</h2>

// Card / panel heading
<h3 className="text-base font-medium text-foreground">
  Agreement Details
</h3>

// Metadata label
<p className="text-sm text-muted-foreground">Last updated 3 days ago</p>
```

### Letter Spacing

- Headings: `tracking-tight` (`-0.025em`)
- Body: default (0)
- Uppercase labels (e.g., table column headers, badge text): `tracking-wide` (`0.025em`) + `uppercase` + `text-xs`

---

## 5. Spacing System

### Base Unit

The spacing system uses a **4px base unit**. All padding, margin, gap, and sizing values must be multiples of 4px. Use Tailwind spacing utilities exclusively.

### Spacing Scale Reference

| Tailwind Class | Value  | Pixels | Common Use                                |
|----------------|--------|--------|-------------------------------------------|
| `p-1`          | 0.25rem| 4px    | Icon padding, badge internal padding      |
| `p-2`          | 0.5rem | 8px    | Button padding (sm), input padding        |
| `p-3`          | 0.75rem| 12px   | Card internal padding (compact)           |
| `p-4`          | 1rem   | 16px   | Card padding (standard), form group gap   |
| `p-6`          | 1.5rem | 24px   | Page section padding, dialog content      |
| `p-8`          | 2rem   | 32px   | Page-level horizontal padding             |
| `gap-2`        | 0.5rem | 8px    | Icon + label gap, inline tag gap          |
| `gap-4`        | 1rem   | 16px   | Form field gap, stat card grid gap        |
| `gap-6`        | 1.5rem | 24px   | Section-to-section gap within a page      |
| `gap-8`        | 2rem   | 32px   | Dashboard grid column gap                 |

### Padding Conventions

- **Page container:** `px-6 py-6` (desktop), `px-4 py-4` (mobile)
- **Card:** `p-6` standard, `p-4` compact (dense tables)
- **Dialog content:** `px-6 pb-6 pt-0`
- **Table cell:** `px-4 py-3`
- **Button (default):** handled by shadcn/ui `h-10 px-4 py-2`
- **Sidebar item:** `px-3 py-2`

---

## 6. Icons

### Approved Library

**lucide-react** is the only approved icon library. Do not use react-icons, heroicons, Font Awesome, or emoji as UI icons.

```bash
# Already in dependencies — do not reinstall
import { FileText, Globe, Settings, ChevronRight } from 'lucide-react'
```

### Size Standards

| Context                          | Size Class       | Pixels |
|----------------------------------|------------------|--------|
| Inline text icons                | `size-4`         | 16px   |
| Button icons (default button)    | `size-4`         | 16px   |
| Navigation sidebar icons         | `size-5`         | 20px   |
| Section / card header icons      | `size-5`         | 20px   |
| Empty state illustrations        | `size-12`        | 48px   |
| Error page illustrations         | `size-16`        | 64px   |

### Usage Pattern

```tsx
// Always pair with aria-hidden when decorative
<FileText className="size-4 shrink-0" aria-hidden="true" />

// When icon conveys meaning (standalone), add sr-only label
<button aria-label="Download report">
  <Download className="size-4" aria-hidden="true" />
</button>

// Icon + text — standard spacing
<span className="flex items-center gap-2">
  <Globe className="size-4 shrink-0 text-muted-foreground" aria-hidden="true" />
  <span>Countries</span>
</span>
```

### Rules

- `shrink-0` on icons prevents them from collapsing in flex containers.
- Use `text-muted-foreground` for decorative/secondary icons, `text-foreground` for primary, `text-primary` for interactive.
- No emoji in the UI. Emoji in user-generated data (HS code descriptions) may be displayed but not styled.

---

## 7. Buttons

All buttons use the shadcn/ui `Button` component from `components/ui/button.tsx`.

### Variants

```tsx
import { Button } from '@/components/ui/button'

// Default — primary action (blue fill)
<Button variant="default">Calculate Origin</Button>

// Destructive — irreversible actions (red fill)
<Button variant="destructive">Delete Agreement</Button>

// Outline — secondary action
<Button variant="outline">Cancel</Button>

// Ghost — tertiary / icon-only actions in toolbars
<Button variant="ghost" size="icon" aria-label="Edit">
  <Pencil className="size-4" />
</Button>

// Link — in-text navigation
<Button variant="link">View full report</Button>
```

### Size Variants

```tsx
<Button size="sm">Export</Button>    {/* h-9  px-3 text-sm */}
<Button size="default">Save</Button> {/* h-10 px-4 py-2    */}
<Button size="lg">Start Calculation</Button> {/* h-11 px-8 */}
<Button size="icon">                 {/* h-10 w-10 — icon only */}
  <Plus className="size-4" />
</Button>
```

### Loading State

```tsx
import { Loader2 } from 'lucide-react'

<Button disabled={isLoading}>
  {isLoading && <Loader2 className="size-4 animate-spin mr-2" aria-hidden="true" />}
  {isLoading ? 'Calculating…' : 'Calculate Origin'}
</Button>
```

### Rules

- The **primary action** button in any dialog or form is always `variant="default"`.
- Destructive confirmation dialogs use `variant="destructive"` on the confirm button.
- Cancel / dismiss is always `variant="outline"`.
- Never nest interactive elements inside `<Button>`.
- Disabled state is communicated via `disabled` prop — do not fake it with opacity CSS alone.

---

## 8. Forms

### Standard Field Pattern

```tsx
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Form, FormControl, FormDescription, FormField,
  FormItem, FormLabel, FormMessage,
} from '@/components/ui/form'
import { Input } from '@/components/ui/input'

// Every field follows: Label → Control → Description → Error
<FormField
  control={form.control}
  name="hsCode"
  render={({ field }) => (
    <FormItem>
      <FormLabel>
        HS Code <span className="text-destructive" aria-label="required">*</span>
      </FormLabel>
      <FormControl>
        <Input placeholder="e.g. 8471.30" {...field} />
      </FormControl>
      <FormDescription>
        6-digit Harmonised System code for the finished product.
      </FormDescription>
      <FormMessage /> {/* Renders Zod validation error */}
    </FormItem>
  )}
/>
```

### Validation Approach

- All schemas live in `frontend/src/schemas/`. Import into the feature component.
- Use **inline validation** triggered on `onBlur` for long forms (React Hook Form default `mode: 'onBlur'`).
- For wizard steps, validate the step schema on "Next" click before advancing.
- Required field indicator: red asterisk `*` rendered as `<span className="text-destructive">*</span>`, accompanied by a legend at the top of the form: "Fields marked * are required."

### Character Count

```tsx
// For textareas with length limits
<div className="relative">
  <Textarea
    {...field}
    maxLength={500}
    className="resize-none"
  />
  <span className="absolute bottom-2 right-3 text-xs text-muted-foreground">
    {field.value?.length ?? 0}/500
  </span>
</div>
```

### Form Layout

- Single-column layout for forms with fewer than 6 fields.
- Two-column grid (`grid grid-cols-2 gap-4`) for forms with 6–12 fields on desktop; collapse to single column at `sm` breakpoint.
- Group related fields under a `<Separator />` with a group label.

---

## 9. Dialogs

### Dialog vs AlertDialog

| Scenario                                   | Component       |
|--------------------------------------------|-----------------|
| Forms, detail views, multi-step inputs     | `Dialog`        |
| Irreversible destructive actions (delete)  | `AlertDialog`   |
| Informational confirmations                | `Dialog`        |

### Width Conventions

```tsx
// Small — confirmation messages
<DialogContent className="max-w-sm">

// Default — standard forms
<DialogContent className="max-w-lg">

// Medium — detail panels, rule editors
<DialogContent className="max-w-2xl">

// Large — complex forms, material selection
<DialogContent className="max-w-4xl">
```

### Footer Button Order

Cancel is always on the left; the primary (confirm/submit) action is always on the right. This mirrors platform conventions and prevents accidental destructive clicks.

```tsx
<DialogFooter className="gap-2 sm:justify-end">
  <DialogClose asChild>
    <Button variant="outline">Cancel</Button>
  </DialogClose>
  <Button type="submit" disabled={isSubmitting}>
    {isSubmitting && <Loader2 className="size-4 animate-spin mr-2" />}
    Save Agreement
  </Button>
</DialogFooter>
```

### AlertDialog for Destructive Actions

```tsx
<AlertDialog>
  <AlertDialogTrigger asChild>
    <Button variant="destructive">Delete Rule</Button>
  </AlertDialogTrigger>
  <AlertDialogContent>
    <AlertDialogHeader>
      <AlertDialogTitle>Delete this rule?</AlertDialogTitle>
      <AlertDialogDescription>
        This action cannot be undone. The rule will be permanently removed
        from all associated trade agreements.
      </AlertDialogDescription>
    </AlertDialogHeader>
    <AlertDialogFooter>
      <AlertDialogCancel>Cancel</AlertDialogCancel>
      <AlertDialogAction
        className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
        onClick={handleDelete}
      >
        Delete Rule
      </AlertDialogAction>
    </AlertDialogFooter>
  </AlertDialogContent>
</AlertDialog>
```

---

## 10. Tables and DataTable

### Stack

All data tables use **TanStack Table v8** for logic and **shadcn/ui** primitives (`Table`, `TableHeader`, `TableRow`, `TableHead`, `TableBody`, `TableCell`) for rendering.

The reusable `DataTable` component lives at `frontend/src/components/ui/data-table.tsx`.

### Feature Checklist

| Feature              | Implementation                                              |
|----------------------|-------------------------------------------------------------|
| Sorting              | `getSortedRowModel()`, `SortingState`, column header click  |
| Column filtering     | `getFilteredRowModel()`, per-column `Filter` input          |
| Global search        | `globalFilter` state, `Input` in toolbar                   |
| Pagination           | `getPaginationRowModel()`, page size selector               |
| Row selection        | `getIsAllRowsSelected()`, checkbox column                   |
| Bulk actions         | Action bar appears when `rowSelection` is non-empty         |
| Column visibility    | `DropdownMenu` toggle via `column.toggleVisibility()`       |
| Column pinning       | `columnPinning` state, sticky left/right classes            |
| Skeleton loading     | Render `SkeletonRows` when `isLoading === true`             |
| Export               | Buttons: Excel (xlsx), PDF (jsPDF), CSV (papaparse)         |

### DataTable Usage

```tsx
import { DataTable } from '@/components/ui/data-table'
import { columns } from './agreements-columns'

<DataTable
  columns={columns}
  data={agreements}
  isLoading={isLoading}
  filterableColumns={[
    { id: 'status', title: 'Status', options: statusOptions },
  ]}
  searchableColumns={[{ id: 'name', title: 'Agreement Name' }]}
  exportFileName="trade-agreements"
/>
```

### Column Definition Example

```tsx
// features/agreements/agreements-columns.tsx
import { ColumnDef } from '@tanstack/react-table'
import { Badge } from '@/components/ui/badge'
import { DataTableColumnHeader } from '@/components/ui/data-table-column-header'

export const columns: ColumnDef<TradeAgreement>[] = [
  {
    id: 'select',
    header: ({ table }) => (
      <Checkbox
        checked={table.getIsAllPageRowsSelected()}
        onCheckedChange={(v) => table.toggleAllPageRowsSelected(!!v)}
        aria-label="Select all"
      />
    ),
    cell: ({ row }) => (
      <Checkbox
        checked={row.getIsSelected()}
        onCheckedChange={(v) => row.toggleSelected(!!v)}
        aria-label="Select row"
      />
    ),
    enableSorting: false,
    enableHiding: false,
  },
  {
    accessorKey: 'name',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Agreement Name" />
    ),
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => {
      const status = row.getValue<string>('status')
      return (
        <Badge variant={status === 'active' ? 'default' : 'secondary'}>
          {status}
        </Badge>
      )
    },
  },
]
```

### Skeleton Loading State

```tsx
// Render when isLoading is true
function SkeletonRows({ columns, rows = 8 }: { columns: number; rows?: number }) {
  return Array.from({ length: rows }).map((_, i) => (
    <TableRow key={i}>
      {Array.from({ length: columns }).map((_, j) => (
        <TableCell key={j}>
          <Skeleton className="h-4 w-full" />
        </TableCell>
      ))}
    </TableRow>
  ))
}
```

### Mobile Horizontal Scroll

Wrap the table in an overflow container. Never let the table break the page layout:

```tsx
<div className="rounded-md border overflow-x-auto">
  <Table className="min-w-[640px]">
    {/* ... */}
  </Table>
</div>
```

---

## 11. Searchable Dropdowns

Use the shadcn/ui **Combobox** pattern (built from `Popover` + `Command`) for all Agreement, Country, and HS Code selectors. These are high-cardinality lists that require search-to-filter.

```tsx
// components/ui/combobox.tsx — reusable wrapper
import { useState } from 'react'
import { Check, ChevronsUpDown } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem } from '@/components/ui/command'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'

interface ComboboxProps<T extends string> {
  options: { value: T; label: string }[]
  value: T | ''
  onSelect: (value: T) => void
  placeholder?: string
  searchPlaceholder?: string
  emptyMessage?: string
  disabled?: boolean
}

export function Combobox<T extends string>({
  options, value, onSelect,
  placeholder = 'Select…',
  searchPlaceholder = 'Search…',
  emptyMessage = 'No results found.',
  disabled,
}: ComboboxProps<T>) {
  const [open, setOpen] = useState(false)
  const label = options.find((o) => o.value === value)?.label

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="w-full justify-between font-normal"
          disabled={disabled}
        >
          {label ?? <span className="text-muted-foreground">{placeholder}</span>}
          <ChevronsUpDown className="size-4 shrink-0 opacity-50 ml-2" aria-hidden="true" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-full p-0" align="start">
        <Command>
          <CommandInput placeholder={searchPlaceholder} />
          <CommandEmpty>{emptyMessage}</CommandEmpty>
          <CommandGroup className="max-h-60 overflow-y-auto">
            {options.map((option) => (
              <CommandItem
                key={option.value}
                value={option.value}
                onSelect={() => { onSelect(option.value as T); setOpen(false) }}
              >
                <Check
                  className={cn('size-4 mr-2', value === option.value ? 'opacity-100' : 'opacity-0')}
                  aria-hidden="true"
                />
                {option.label}
              </CommandItem>
            ))}
          </CommandGroup>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
```

---

## 12. Multi-Select Controls

For fields requiring multiple selections (applicable countries, material selection in the calculator), use a multi-select built from `Command` + `Badge` tags.

```tsx
// Pattern: selected items shown as dismissible Badge chips above the trigger
<div className="space-y-2">
  <div className="flex flex-wrap gap-1 min-h-[2rem]">
    {selectedCountries.map((code) => (
      <Badge key={code} variant="secondary" className="gap-1">
        {countryLabel(code)}
        <button
          type="button"
          aria-label={`Remove ${countryLabel(code)}`}
          onClick={() => removeCountry(code)}
          className="rounded-full outline-none ring-offset-background focus:ring-2 focus:ring-ring focus:ring-offset-1"
        >
          <X className="size-3" aria-hidden="true" />
        </button>
      </Badge>
    ))}
  </div>
  <Popover>
    <PopoverTrigger asChild>
      <Button variant="outline" className="w-full justify-start font-normal text-muted-foreground">
        <Plus className="size-4 mr-2" aria-hidden="true" /> Add countries…
      </Button>
    </PopoverTrigger>
    <PopoverContent className="p-0 w-72" align="start">
      <Command>
        <CommandInput placeholder="Search countries…" />
        <CommandList className="max-h-52">
          <CommandEmpty>No country found.</CommandEmpty>
          <CommandGroup>
            {allCountries.map((c) => (
              <CommandItem key={c.code} onSelect={() => toggleCountry(c.code)}>
                <Check
                  className={cn('size-4 mr-2', selectedCountries.includes(c.code) ? 'opacity-100' : 'opacity-0')}
                  aria-hidden="true"
                />
                {c.name}
              </CommandItem>
            ))}
          </CommandGroup>
        </CommandList>
      </Command>
    </PopoverContent>
  </Popover>
</div>
```

---

## 13. Cards

### Standard Card Structure

```tsx
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'

<Card>
  <CardHeader>
    <CardTitle>EU-Japan EPA</CardTitle>
    <CardDescription>Economic Partnership Agreement · In force since 2019</CardDescription>
  </CardHeader>
  <CardContent>
    {/* Primary content */}
  </CardContent>
  <CardFooter className="justify-between text-sm text-muted-foreground">
    <span>Last updated 3 days ago</span>
    <Button variant="ghost" size="sm">View details</Button>
  </CardFooter>
</Card>
```

### Density Variants

| Variant  | Padding | Use Case                                        |
|----------|---------|-------------------------------------------------|
| Standard | `p-6`   | Detail cards, form-containing cards             |
| Compact  | `p-4`   | List-style cards, cards inside scrollable panels|
| Flush    | `p-0`   | Cards containing a full-width table or map      |

### Dashboard Stat Cards

```tsx
// features/dashboard/StatCard.tsx
interface StatCardProps {
  title: string
  value: string | number
  description: string
  icon: LucideIcon
  trend?: { value: number; positive: boolean }
  sparklineData?: number[]
}

function StatCard({ title, value, description, icon: Icon, trend }: StatCardProps) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">{title}</CardTitle>
        <Icon className="size-5 text-muted-foreground" aria-hidden="true" />
      </CardHeader>
      <CardContent>
        <div className="text-3xl font-bold text-foreground">{value}</div>
        <p className="text-xs text-muted-foreground mt-1 flex items-center gap-1">
          {trend && (
            <span className={cn('font-medium', trend.positive ? 'text-success' : 'text-destructive')}>
              {trend.positive ? <TrendingUp className="size-3 inline" /> : <TrendingDown className="size-3 inline" />}
              {Math.abs(trend.value)}%
            </span>
          )}
          {description}
        </p>
      </CardContent>
    </Card>
  )
}
```

Standard dashboard stat cards:

| Card Title          | Icon          | Metric                          |
|---------------------|---------------|---------------------------------|
| Total Calculations  | `Calculator`  | Cumulative count + monthly trend|
| Active Agreements   | `FileCheck`   | Count of `status: active`       |
| Pass Rate           | `CheckCircle` | % originating + sparkline       |
| Pending Reviews     | `Clock`       | Count awaiting manual review    |

---

## 14. Navigation

### Sidebar

The sidebar is the primary navigation container. It is collapsible (icon-only mode) on desktop and slides in as a drawer on mobile.

```
Sidebar structure:
├── Logo / App name (top)
├── Navigation groups
│   ├── [Group label: OVERVIEW]
│   │   └── Dashboard
│   ├── [Group label: MASTER DATA]
│   │   ├── Countries
│   │   ├── Trade Agreements
│   │   ├── HS Codes
│   │   ├── Rules
│   │   ├── Products
│   │   └── Materials
│   ├── [Group label: OPERATIONS]
│   │   ├── Origin Calculator
│   │   └── Reports
│   └── [Group label: SYSTEM]
│       └── Settings
└── User avatar / account (bottom)
```

**Sidebar item active state:**

```tsx
// Use cn() to conditionally apply active styles
<NavLink to="/agreements">
  {({ isActive }) => (
    <span className={cn(
      'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
      isActive
        ? 'bg-primary text-primary-foreground'
        : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
    )}>
      <FileCheck className="size-5 shrink-0" aria-hidden="true" />
      Trade Agreements
    </span>
  )}
</NavLink>
```

### TopBar

The top bar is a fixed header above the content area containing:

1. **Breadcrumb** — current location using React Router + shadcn/ui `Breadcrumb`
2. **Notifications bell** — `Bell` icon with unread count badge
3. **User menu** — `DropdownMenu` with avatar, user name, role, and Sign Out

```tsx
<header className="sticky top-0 z-30 flex h-14 items-center gap-4 border-b bg-background px-6">
  <Breadcrumb>
    <BreadcrumbList>
      <BreadcrumbItem>
        <BreadcrumbLink href="/agreements">Trade Agreements</BreadcrumbLink>
      </BreadcrumbItem>
      <BreadcrumbSeparator />
      <BreadcrumbItem>
        <BreadcrumbPage>EU-Japan EPA</BreadcrumbPage>
      </BreadcrumbItem>
    </BreadcrumbList>
  </Breadcrumb>

  <div className="ml-auto flex items-center gap-3">
    {/* Notifications */}
    <Button variant="ghost" size="icon" aria-label="Notifications">
      <Bell className="size-5" aria-hidden="true" />
    </Button>

    {/* User menu */}
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" className="relative h-8 w-8 rounded-full">
          <Avatar className="h-8 w-8">
            <AvatarFallback>VK</AvatarFallback>
          </Avatar>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuLabel>Vinod Kumar</DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem>Profile</DropdownMenuItem>
        <DropdownMenuItem>Settings</DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem className="text-destructive">Sign out</DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  </div>
</header>
```

---

## 15. Dashboard Layout

### Responsive Grid

The dashboard uses a 12-column CSS grid:

```tsx
// pages/DashboardPage.tsx
<div className="space-y-6 p-6">
  {/* Stat cards — 4 columns on lg, 2 on sm, 1 on xs */}
  <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
    <StatCard title="Total Calculations" value="1,284" ... />
    <StatCard title="Active Agreements" value="12" ... />
    <StatCard title="Pass Rate" value="87.4%" ... />
    <StatCard title="Pending Reviews" value="5" ... />
  </div>

  {/* Charts row — 2/3 + 1/3 split */}
  <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
    <Card className="lg:col-span-2">
      <CardHeader>
        <CardTitle>Calculation History</CardTitle>
      </CardHeader>
      <CardContent>
        <CalculationHistoryChart />
      </CardContent>
    </Card>
    <Card>
      <CardHeader>
        <CardTitle>Results by Agreement</CardTitle>
      </CardHeader>
      <CardContent>
        <AgreementBreakdownChart />
      </CardContent>
    </Card>
  </div>

  {/* Recent calculations table — full width */}
  <Card>
    <CardHeader>
      <CardTitle>Recent Calculations</CardTitle>
    </CardHeader>
    <CardContent className="p-0">
      <RecentCalculationsTable />
    </CardContent>
  </Card>
</div>
```

---

## 16. Origin Calculator Stepper / Wizard

### Overview

The Origin Calculator is a 5-step wizard located at `frontend/src/features/calculator/`. It is the most complex UI in the application. It guides the user through selecting a product, defining its materials, choosing the applicable trade agreement, reviewing all inputs, and viewing the calculation result.

### Step Structure

```
Step 1: Select Finished Product
Step 2: Add / Review Materials
Step 3: Select Trade Agreement + Destination Country
Step 4: Review Inputs
Step 5: Calculation Result (with Rule Visualization)
```

### Stepper Component

```tsx
// features/calculator/OriginCalculatorStepper.tsx
const STEPS = [
  { id: 1, label: 'Select Product',  icon: Package },
  { id: 2, label: 'Add Materials',   icon: Layers },
  { id: 3, label: 'Agreement',       icon: FileCheck },
  { id: 4, label: 'Review',          icon: ClipboardCheck },
  { id: 5, label: 'Result',          icon: CheckCircle },
]

// Progress indicator — horizontal on desktop, compact on mobile
<nav aria-label="Calculator progress" className="flex items-center justify-between">
  {STEPS.map((step, index) => (
    <React.Fragment key={step.id}>
      <div className="flex flex-col items-center gap-1">
        <div className={cn(
          'flex h-10 w-10 items-center justify-center rounded-full border-2 transition-colors',
          currentStep > step.id && 'bg-primary border-primary text-primary-foreground',
          currentStep === step.id && 'border-primary text-primary bg-background',
          currentStep < step.id && 'border-border text-muted-foreground bg-background',
        )}>
          {currentStep > step.id
            ? <Check className="size-5" aria-hidden="true" />
            : <step.icon className="size-5" aria-hidden="true" />
          }
        </div>
        <span className={cn(
          'hidden sm:block text-xs font-medium',
          currentStep === step.id ? 'text-primary' : 'text-muted-foreground'
        )}>
          {step.label}
        </span>
      </div>
      {index < STEPS.length - 1 && (
        <div className={cn(
          'flex-1 h-0.5 mx-2',
          currentStep > step.id + 1 ? 'bg-primary' : 'bg-border'
        )} aria-hidden="true" />
      )}
    </React.Fragment>
  ))}
</nav>
```

### Step Validation Before Advance

Each step has a Zod schema. Validation runs on "Next" click. If invalid, errors are shown inline and the step does not advance.

```typescript
// schemas/calculator.ts
export const step1Schema = z.object({
  productId: z.string().min(1, 'Select a product to continue'),
})

export const step2Schema = z.object({
  materials: z.array(z.object({
    materialId: z.string().min(1),
    quantity: z.number().positive('Quantity must be positive'),
    unitCost: z.number().nonnegative(),
    originCountryCode: z.string().length(2),
  })).min(1, 'Add at least one material'),
})

export const step3Schema = z.object({
  agreementId: z.string().min(1, 'Select a trade agreement'),
  destinationCountryCode: z.string().min(1, 'Select a destination country'),
})
```

```tsx
// In the step component
async function handleNext() {
  const result = await form.trigger() // validates current step fields
  if (!result) return // stay on step, errors are displayed
  goToNextStep()
}
```

### Back Navigation

Users can always navigate backwards. The wizard state is preserved across steps (React Hook Form context wraps the entire wizard):

```tsx
<Button variant="outline" onClick={goToPrevStep} disabled={currentStep === 1}>
  <ChevronLeft className="size-4 mr-2" aria-hidden="true" /> Back
</Button>
```

### Step 5 — Result Panel

The result step renders the `RuleVisualization` component (see Section 17) and a final decision banner:

```tsx
// Originating — green hero banner
<div className="rounded-lg border-2 border-success bg-success/10 p-6 text-center">
  <CheckCircle className="size-12 text-success mx-auto mb-3" aria-hidden="true" />
  <h2 className="text-2xl font-bold text-success">Originating</h2>
  <p className="text-muted-foreground mt-1">
    Product qualifies for preferential tariff treatment under {agreementName}
  </p>
</div>

// Non-Originating — red hero banner
<div className="rounded-lg border-2 border-destructive bg-destructive/10 p-6 text-center">
  <XCircle className="size-12 text-destructive mx-auto mb-3" aria-hidden="true" />
  <h2 className="text-2xl font-bold text-destructive">Non-Originating</h2>
  <p className="text-muted-foreground mt-1">
    Product does not qualify. See failed rules below.
  </p>
</div>
```

---

## 17. Rule Visualization Component

### Purpose

The `RuleVisualization` component displays the step-by-step execution of origin rules against the calculator inputs. It lives at `frontend/src/features/calculator/RuleVisualization.tsx`.

### Rule Result Item

```tsx
interface RuleResult {
  ruleCode: string
  ruleName: string
  status: 'passed' | 'failed' | 'skipped'
  executionTimeMs: number
  calculatedValue?: number
  threshold?: number
  unit?: string
  details?: string
}

const statusConfig = {
  passed:  { label: 'Passed',  badgeClass: 'bg-success/15 text-success border-success/30',  icon: CheckCircle },
  failed:  { label: 'Failed',  badgeClass: 'bg-destructive/15 text-destructive border-destructive/30', icon: XCircle },
  skipped: { label: 'Skipped', badgeClass: 'bg-muted text-muted-foreground border-border', icon: Minus },
}

function RuleResultItem({ rule }: { rule: RuleResult }) {
  const [expanded, setExpanded] = useState(false)
  const config = statusConfig[rule.status]
  const Icon = config.icon

  return (
    <div className="rounded-md border bg-card">
      <button
        type="button"
        className="flex w-full items-center justify-between p-4 text-left"
        onClick={() => setExpanded(!expanded)}
        aria-expanded={expanded}
      >
        <div className="flex items-center gap-3">
          <Icon className="size-5 shrink-0" aria-hidden="true" />
          <div>
            <span className="text-sm font-medium text-foreground">{rule.ruleName}</span>
            <span className="ml-2 text-xs text-muted-foreground font-mono">{rule.ruleCode}</span>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <Badge className={cn('text-xs border', config.badgeClass)}>{config.label}</Badge>
          <span className="text-xs text-muted-foreground">{rule.executionTimeMs}ms</span>
          <ChevronDown className={cn('size-4 transition-transform text-muted-foreground', expanded && 'rotate-180')} aria-hidden="true" />
        </div>
      </button>

      {expanded && (
        <div className="border-t px-4 pb-4 pt-3 space-y-2">
          {rule.calculatedValue !== undefined && rule.threshold !== undefined && (
            <div className="flex items-center gap-2 text-sm">
              <span className="text-muted-foreground">Calculated:</span>
              <span className="font-mono font-medium">
                {rule.calculatedValue.toFixed(2)}{rule.unit}
              </span>
              <span className="text-muted-foreground">vs threshold</span>
              <span className="font-mono font-medium">
                {rule.threshold.toFixed(2)}{rule.unit}
              </span>
            </div>
          )}
          {rule.details && (
            <p className="text-sm text-muted-foreground">{rule.details}</p>
          )}
        </div>
      )}
    </div>
  )
}
```

### Decision Tree Layout

Rules are presented as a vertical list with connecting lines to suggest sequential evaluation. Group rules by type (CTH, RVC, SP) with `Separator` + group label.

```tsx
function RuleVisualization({ results }: { results: RuleResult[] }) {
  return (
    <div className="space-y-2">
      <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
        Rule Execution Trace
      </h3>
      <div className="relative space-y-2 pl-4 before:absolute before:left-[7px] before:top-0 before:h-full before:w-px before:bg-border">
        {results.map((rule) => (
          <div key={rule.ruleCode} className="relative">
            <div className="absolute -left-4 top-1/2 h-px w-4 bg-border" aria-hidden="true" />
            <RuleResultItem rule={rule} />
          </div>
        ))}
      </div>
    </div>
  )
}
```

---

## 18. Rule Builder UI

The Rule Builder is used in the Rules module to create and edit origin rules. It lives at `frontend/src/features/rules/RuleBuilder.tsx`.

### Layout

```
┌─────────────────────────────────────────────────────────┐
│  Rule Metadata (left 2/3)    │  Test Panel (right 1/3)  │
│  ─ Rule name                 │  ─ Sample input JSON      │
│  ─ Rule code                 │  ─ [Run Test] button      │
│  ─ Rule type (CTH/RVC/SP)    │  ─ Test result output     │
│  ─ Description               │                           │
│  ─ Parameters (JSON editor)  │                           │
│  ─ Applicable agreements     │                           │
└─────────────────────────────────────────────────────────┘
```

### JSON Parameter Editor

Use a `Textarea` with monospace font for the JSON parameters field. Validate JSON on blur:

```tsx
<FormField
  name="parameters"
  render={({ field }) => (
    <FormItem>
      <FormLabel>Rule Parameters (JSON)</FormLabel>
      <FormControl>
        <Textarea
          {...field}
          className="font-mono text-sm min-h-[160px] resize-y"
          placeholder='{"threshold": 40, "unit": "percent", "valueType": "ex-works"}'
          onBlur={(e) => {
            try {
              JSON.parse(e.target.value)
              form.clearErrors('parameters')
            } catch {
              form.setError('parameters', { message: 'Invalid JSON syntax' })
            }
            field.onBlur()
          }}
        />
      </FormControl>
      <FormMessage />
    </FormItem>
  )}
/>
```

---

## 19. Responsive Rules

### Breakpoints (Tailwind CSS 4 Defaults)

| Prefix | Min Width | Target Devices                   |
|--------|-----------|----------------------------------|
| `sm:`  | 640px     | Large phones (landscape), tablets|
| `md:`  | 768px     | Tablets (portrait)               |
| `lg:`  | 1024px    | Laptops, small desktops          |
| `xl:`  | 1280px    | Standard desktops                |
| `2xl:` | 1536px    | Wide monitors                    |

### Mobile-First Strategy

Write base styles for mobile, then add responsive prefixes for larger screens. Never write desktop-first CSS.

```tsx
// Correct — mobile first
<div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">

// Incorrect — desktop first (do not do this)
<div className="grid grid-cols-4 gap-4 lg:grid-cols-4 sm:grid-cols-2 grid-cols-1">
```

### Table Scroll on Mobile

All tables are wrapped in a horizontal scroll container (see Section 10). The `min-w-[640px]` class ensures the table maintains readable column widths on mobile without breaking layout.

### Sidebar on Mobile

On viewports below `lg:`, the sidebar collapses to a drawer (shadcn/ui `Sheet`). A hamburger button in the TopBar triggers it.

### Hiding Elements on Mobile

Use `hidden sm:block` or `hidden lg:flex`. Do not use `invisible` or `opacity-0` to hide layout-affecting elements on mobile — this wastes layout space.

---

## 20. Accessibility

### ARIA Labels

Every interactive element that lacks visible text must have an `aria-label`. Use `aria-describedby` to link form controls to their error messages.

```tsx
// Good — icon button
<Button variant="ghost" size="icon" aria-label="Export to Excel">
  <FileSpreadsheet className="size-4" aria-hidden="true" />
</Button>

// Good — error linkage (shadcn/ui FormMessage handles this automatically)
<Input aria-describedby="email-error" />
<p id="email-error" role="alert">{error}</p>
```

### Focus Visible Styles

Do not remove `:focus-visible` outlines. The project global CSS must include:

```css
/* frontend/src/index.css */
:focus-visible {
  outline: 2px solid hsl(var(--ring));
  outline-offset: 2px;
}
```

The shadcn/ui components ship this via the `ring` utility classes. Verify that no component applies `focus:outline-none` without replacing it with `focus-visible:ring-2 focus-visible:ring-ring`.

### Keyboard Navigation

- All dialogs trap focus while open (`Dialog` from shadcn/ui does this by default via Radix UI).
- Dropdowns and comboboxes are navigable via arrow keys.
- Tables support keyboard row navigation. Selected rows are announced via `aria-selected`.
- The wizard stepper's "Next" and "Back" buttons are always focusable and never visually hidden.

### Colour Contrast

- Normal text on background: 4.5:1 minimum (WCAG AA). `text-foreground` on `bg-background` passes.
- Large text (18px+ or 14px+ bold): 3:1 minimum.
- The `text-muted-foreground` (`#64748b`) on `bg-background` (`#ffffff`) achieves approximately 4.6:1 — acceptable.
- **Never** use `text-muted-foreground` on `bg-muted` backgrounds for important content — contrast drops below 3:1.

### Screen Reader Announcements

Use `role="status"` or `role="alert"` for dynamic content:

```tsx
// Loading state announcement
<div role="status" aria-live="polite" aria-label="Loading data">
  <SkeletonTable />
</div>

// Error announcement
<div role="alert">
  <p className="text-destructive">Failed to load agreements. Please try again.</p>
</div>
```

---

## 21. Loading States

### Skeleton Components

Every data-fetching component has a skeleton counterpart. Use `Skeleton` from `components/ui/skeleton`.

```tsx
// DataTable skeleton — see Section 10
// Card skeleton
function StatCardSkeleton() {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <Skeleton className="h-4 w-32" />
        <Skeleton className="h-5 w-5 rounded" />
      </CardHeader>
      <CardContent>
        <Skeleton className="h-8 w-20 mb-2" />
        <Skeleton className="h-3 w-40" />
      </CardContent>
    </Card>
  )
}
```

### Suspense Boundaries

Each route-level component is wrapped in a `Suspense` boundary so navigation chrome renders immediately:

```tsx
// pages/AgreementsPage.tsx
import { Suspense } from 'react'

export default function AgreementsPage() {
  return (
    <Suspense fallback={<AgreementsTableSkeleton />}>
      <AgreementsTable />
    </Suspense>
  )
}
```

### Optimistic Updates

For low-risk mutations (toggling active/inactive status, adding a tag), apply optimistic updates via TanStack Query's `onMutate` callback. Roll back on error and show an error toast.

---

## 22. Notifications

### Library

**sonner** is the approved toast library. Do not use react-toastify or custom toast implementations.

```bash
# Already in dependencies
import { toast } from 'sonner'
import { Toaster } from 'sonner'
```

### Setup

Place `<Toaster />` once in the root layout:

```tsx
// App.tsx or root layout
<Toaster
  position="top-right"
  richColors
  expand={false}
  duration={4000}
/>
```

### Variants and Usage

```typescript
// Success — green
toast.success('Calculation complete', {
  description: 'Product qualifies as originating under EU-Japan EPA.',
})

// Error — red
toast.error('Calculation failed', {
  description: 'Unable to reach the calculation engine. Please try again.',
})

// Warning — amber
toast.warning('Incomplete data', {
  description: '3 materials are missing origin country information.',
})

// Info — blue
toast.info('Agreement updated', {
  description: 'The rules for EU-Canada CETA have been refreshed.',
})

// With action button
toast.error('Save failed', {
  description: 'Your changes were not saved.',
  action: {
    label: 'Retry',
    onClick: () => handleSave(),
  },
})
```

### Rules

- Position: always `top-right`.
- Duration: 4 seconds for success/info, 6 seconds for warning/error.
- Never show toasts for background polling or silent data refreshes.
- Do not show more than 3 toasts simultaneously. Sonner queues automatically.

---

## 23. Error Screens

All error screens follow the same layout: centred content, Lucide icon, heading, description, and a primary action button. They live in `frontend/src/pages/error/`.

```tsx
// pages/error/NotFoundPage.tsx
export function NotFoundPage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
      <FileQuestion className="size-16 text-muted-foreground mb-6" aria-hidden="true" />
      <h1 className="text-4xl font-bold text-foreground mb-2">404</h1>
      <h2 className="text-xl font-semibold text-foreground mb-3">Page not found</h2>
      <p className="text-muted-foreground max-w-sm mb-8">
        The page you are looking for does not exist or has been moved.
      </p>
      <Button asChild>
        <Link to="/dashboard">Go to Dashboard</Link>
      </Button>
    </div>
  )
}
```

| Screen    | Icon              | Heading       | Description                           | CTA                   |
|-----------|-------------------|---------------|---------------------------------------|-----------------------|
| 404       | `FileQuestion`    | 404           | Page not found                        | Go to Dashboard       |
| 500       | `ServerCrash`     | 500           | Something went wrong on our end       | Reload page           |
| Forbidden | `ShieldAlert`     | Access Denied | You don't have permission to view this| Contact administrator |

---

## 24. Empty States

Never display blank space when a list or table has no data. Every empty state has: an icon, a descriptive message explaining why it is empty, and a call-to-action.

```tsx
// Reusable pattern
function EmptyState({
  icon: Icon,
  title,
  description,
  action,
}: {
  icon: LucideIcon
  title: string
  description: string
  action?: { label: string; onClick: () => void }
}) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center px-4">
      <Icon className="size-12 text-muted-foreground mb-4" aria-hidden="true" />
      <h3 className="text-lg font-semibold text-foreground mb-1">{title}</h3>
      <p className="text-sm text-muted-foreground max-w-xs mb-6">{description}</p>
      {action && (
        <Button onClick={action.onClick}>
          <Plus className="size-4 mr-2" aria-hidden="true" /> {action.label}
        </Button>
      )}
    </div>
  )
}
```

| Module            | Empty State Title               | CTA                     |
|-------------------|---------------------------------|-------------------------|
| Trade Agreements  | No agreements yet               | Add Trade Agreement     |
| HS Codes          | No HS codes configured          | Import HS Codes         |
| Rules             | No rules defined                | Create Rule             |
| Products          | No products found               | Add Product             |
| Materials         | No materials added              | Add Material            |
| Reports           | No reports generated            | Run Calculation         |
| Calculator Step 2 | No materials added to this product | Add First Material  |

---

## 25. Data Visualization

### Approved Library

**Recharts** is the only approved charting library. Do not use Chart.js, Victory, or D3 directly in components.

### Chart Type Decision Guide

| Data type                              | Chart type      | Recharts component       |
|----------------------------------------|-----------------|--------------------------|
| Trend over time (calculations/day)     | Area chart      | `AreaChart`              |
| Comparison across agreements           | Bar chart       | `BarChart`               |
| Pass/fail proportion                   | Pie/donut chart | `PieChart`               |
| Pass rate trend over time              | Line chart      | `LineChart`              |
| Sparklines in stat cards               | Line (no axes)  | `LineChart` (minimal)    |

### Chart Colour Palette

Use explicit hex values in Recharts (CSS variables are not supported in SVG fill attributes):

```typescript
// lib/chart-colors.ts
export const CHART_COLORS = {
  primary:     '#3b82f6', // blue-500
  success:     '#16a34a', // green-600
  destructive: '#ef4444', // red-500
  warning:     '#f59e0b', // amber-500
  muted:       '#94a3b8', // slate-400
  series: [
    '#3b82f6', '#8b5cf6', '#06b6d4', '#f59e0b', '#10b981', '#f43f5e',
  ],
}
```

### Standard Chart Wrapper

```tsx
// Wrap all charts in a Card with consistent height
<Card>
  <CardHeader>
    <CardTitle>Calculation History</CardTitle>
    <CardDescription>Daily calculations over the last 30 days</CardDescription>
  </CardHeader>
  <CardContent>
    <ResponsiveContainer width="100%" height={300}>
      <AreaChart data={chartData}>
        <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
        <XAxis dataKey="date" tick={{ fontSize: 12, fill: 'hsl(var(--muted-foreground))' }} />
        <YAxis tick={{ fontSize: 12, fill: 'hsl(var(--muted-foreground))' }} />
        <Tooltip
          contentStyle={{
            backgroundColor: 'hsl(var(--background))',
            border: '1px solid hsl(var(--border))',
            borderRadius: '6px',
          }}
        />
        <Area
          type="monotone"
          dataKey="count"
          stroke={CHART_COLORS.primary}
          fill={CHART_COLORS.primary}
          fillOpacity={0.15}
        />
      </AreaChart>
    </ResponsiveContainer>
  </CardContent>
</Card>
```

---

## 26. Animations

### Approved Library

**Framer Motion** is approved for page transitions only. Do not use it for component-level hover effects, list item animations, or decorative motion.

### Page Transitions

```tsx
// components/PageTransition.tsx
import { motion } from 'framer-motion'

const variants = {
  initial:  { opacity: 0, y: 8 },
  animate:  { opacity: 1, y: 0 },
  exit:     { opacity: 0, y: -8 },
}

export function PageTransition({ children }: { children: React.ReactNode }) {
  return (
    <motion.div
      variants={variants}
      initial="initial"
      animate="animate"
      exit="exit"
      transition={{ duration: 0.15, ease: 'easeOut' }}
    >
      {children}
    </motion.div>
  )
}
```

### Reduced Motion

Always respect the user's motion preference:

```tsx
// Disable animation when user prefers reduced motion
import { useReducedMotion } from 'framer-motion'

function PageTransition({ children }) {
  const shouldReduce = useReducedMotion()
  return (
    <motion.div
      initial={shouldReduce ? false : { opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      transition={shouldReduce ? { duration: 0 } : { duration: 0.15 }}
    >
      {children}
    </motion.div>
  )
}
```

### Rules

- Page enter/exit transitions: maximum 200ms duration.
- No skeleton-to-content animation — content replaces the skeleton immediately.
- No hover scale transforms on cards or buttons.
- Accordion / collapsible open/close uses shadcn/ui's built-in CSS animation (not Framer Motion).

---

## 27. Component Naming Conventions

### File Naming

- All component files use **PascalCase**: `RuleVisualization.tsx`, `OriginCalculatorStepper.tsx`.
- Hook files use **camelCase** with `use` prefix: `useCalculatorState.ts`, `useAgreements.ts`.
- Utility files use **camelCase**: `formatCurrency.ts`, `cn.ts`.

### Component Naming Rules

| Rule                      | Correct                         | Incorrect                  |
|---------------------------|---------------------------------|----------------------------|
| PascalCase always         | `DataTable`                     | `datatable`, `data-table`  |
| Feature-prefixed          | `RuleBuilder`, `AgreementForm`  | `Builder`, `Form`          |
| No generic names          | `CountriesTable`                | `Table1`, `MyTable`        |
| No abbreviations          | `OriginCalculator`              | `OrigCalc`, `OC`           |
| Suffix for type clarity   | `AgreementCard`, `RuleDialog`   | `Agreement`, `Rule`        |

### Feature Component Naming Map

| Module            | Component Examples                                          |
|-------------------|-------------------------------------------------------------|
| Dashboard         | `DashboardPage`, `StatCard`, `CalculationHistoryChart`      |
| Countries         | `CountriesPage`, `CountriesTable`, `CountryDetailDialog`    |
| Trade Agreements  | `AgreementsPage`, `AgreementsTable`, `AgreementForm`        |
| HS Codes          | `HsCodesPage`, `HsCodesTable`, `HsCodeImportDialog`         |
| Rules             | `RulesPage`, `RulesTable`, `RuleBuilder`, `RuleTestPanel`   |
| Products          | `ProductsPage`, `ProductsTable`, `ProductForm`              |
| Materials         | `MaterialsPage`, `MaterialsTable`, `MaterialForm`           |
| Origin Calculator | `OriginCalculatorPage`, `OriginCalculatorStepper`, `RuleVisualization`, `CalculatorStep1`, `CalculatorStep2` |
| Reports           | `ReportsPage`, `ReportsTable`, `ReportDetailView`           |
| Settings          | `SettingsPage`, `SettingsForm`, `ApiKeyDialog`              |

---

## 28. Folder Structure

```
frontend/src/
├── components/
│   └── ui/                      # shadcn/ui primitives — do not modify directly
│       ├── button.tsx
│       ├── card.tsx
│       ├── combobox.tsx          # Custom wrapper around Command + Popover
│       ├── data-table.tsx        # Generic DataTable (TanStack Table)
│       ├── data-table-column-header.tsx
│       ├── dialog.tsx
│       ├── form.tsx
│       ├── input.tsx
│       ├── multi-select.tsx      # Custom multi-select built from Command
│       ├── skeleton.tsx
│       └── ...                  # All other shadcn primitives
├── features/
│   ├── agreements/              # Trade Agreement module
│   │   ├── AgreementsPage.tsx
│   │   ├── AgreementsTable.tsx
│   │   ├── AgreementForm.tsx
│   │   └── agreements-columns.tsx
│   ├── calculator/              # Origin Calculator (Stepper/Wizard)
│   │   ├── OriginCalculatorPage.tsx
│   │   ├── OriginCalculatorStepper.tsx
│   │   ├── CalculatorStep1.tsx  # Select Product
│   │   ├── CalculatorStep2.tsx  # Add Materials
│   │   ├── CalculatorStep3.tsx  # Select Agreement + Country
│   │   ├── CalculatorStep4.tsx  # Review Inputs
│   │   ├── CalculatorStep5.tsx  # Result
│   │   └── RuleVisualization.tsx
│   ├── countries/
│   │   ├── CountriesPage.tsx
│   │   ├── CountriesTable.tsx
│   │   └── CountryDetailDialog.tsx
│   ├── dashboard/
│   │   ├── DashboardPage.tsx
│   │   ├── StatCard.tsx
│   │   ├── CalculationHistoryChart.tsx
│   │   └── AgreementBreakdownChart.tsx
│   ├── hscodes/
│   │   ├── HsCodesPage.tsx
│   │   ├── HsCodesTable.tsx
│   │   └── HsCodeImportDialog.tsx
│   ├── materials/
│   │   ├── MaterialsPage.tsx
│   │   ├── MaterialsTable.tsx
│   │   └── MaterialForm.tsx
│   ├── products/
│   │   ├── ProductsPage.tsx
│   │   ├── ProductsTable.tsx
│   │   └── ProductForm.tsx
│   ├── reports/
│   │   ├── ReportsPage.tsx
│   │   ├── ReportsTable.tsx
│   │   └── ReportDetailView.tsx
│   ├── rules/
│   │   ├── RulesPage.tsx
│   │   ├── RulesTable.tsx
│   │   ├── RuleBuilder.tsx
│   │   └── RuleTestPanel.tsx
│   └── settings/
│       ├── SettingsPage.tsx
│       ├── SettingsForm.tsx
│       └── ApiKeyDialog.tsx
├── hooks/                       # Custom React hooks
│   ├── useAgreements.ts
│   ├── useCalculatorState.ts
│   ├── useCountries.ts
│   └── useDebounce.ts
├── lib/                         # Utilities and clients
│   ├── utils.ts                 # cn() and shared helpers
│   ├── api.ts                   # Axios instance and interceptors
│   └── query-client.ts          # TanStack Query client config
├── pages/                       # Route-level wrapper components
│   ├── error/
│   │   ├── NotFoundPage.tsx
│   │   ├── ServerErrorPage.tsx
│   │   └── ForbiddenPage.tsx
│   └── RootLayout.tsx
├── schemas/                     # Zod validation schemas
│   ├── agreement.ts
│   ├── calculator.ts
│   ├── material.ts
│   ├── product.ts
│   └── rule.ts
├── stores/                      # Zustand stores (if needed)
│   └── calculator-store.ts
└── types/                       # TypeScript interfaces and types
    ├── agreement.ts
    ├── calculator.ts
    ├── country.ts
    ├── hscode.ts
    ├── material.ts
    ├── product.ts
    ├── report.ts
    └── rule.ts
```

---

## 29. Design Tokens

### Extending Tailwind Config

Add custom tokens in `tailwind.config.ts`. This makes `--success` and `--warning` available as Tailwind utility classes:

```typescript
// tailwind.config.ts
import type { Config } from 'tailwindcss'

const config: Config = {
  darkMode: ['class'],
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui'],
      },
      colors: {
        background:  'hsl(var(--background))',
        foreground:  'hsl(var(--foreground))',
        primary: {
          DEFAULT:     'hsl(var(--primary))',
          foreground:  'hsl(var(--primary-foreground))',
        },
        secondary: {
          DEFAULT:     'hsl(var(--secondary))',
          foreground:  'hsl(var(--secondary-foreground))',
        },
        muted: {
          DEFAULT:     'hsl(var(--muted))',
          foreground:  'hsl(var(--muted-foreground))',
        },
        accent: {
          DEFAULT:     'hsl(var(--accent))',
          foreground:  'hsl(var(--accent-foreground))',
        },
        destructive: {
          DEFAULT:     'hsl(var(--destructive))',
          foreground:  'hsl(var(--destructive-foreground))',
        },
        border:      'hsl(var(--border))',
        input:       'hsl(var(--input))',
        ring:        'hsl(var(--ring))',
        success:     'hsl(var(--success))',
        warning:     'hsl(var(--warning))',
      },
      borderRadius: {
        lg:  'var(--radius)',
        md:  'calc(var(--radius) - 2px)',
        sm:  'calc(var(--radius) - 4px)',
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
}

export default config
```

### globals.css Structure

```css
/* frontend/src/index.css — top-level structure */
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    /* All CSS variables defined here — see Section 2 */
  }

  .dark {
    /* Dark mode overrides — see Section 3 */
  }

  * {
    @apply border-border;
  }

  body {
    @apply bg-background text-foreground;
    font-feature-settings: "rlig" 1, "calt" 1;
  }

  :focus-visible {
    outline: 2px solid hsl(var(--ring));
    outline-offset: 2px;
  }
}

@layer utilities {
  /* Add custom utilities only if Tailwind has no equivalent */
  .text-balance {
    text-wrap: balance;
  }
}
```

### Token Reference Quick Card

| What you want to style | Class to use              | Do NOT use              |
|------------------------|---------------------------|-------------------------|
| Page background        | `bg-background`           | `bg-white`, `bg-gray-50`|
| Primary text           | `text-foreground`         | `text-slate-900`        |
| Secondary text         | `text-muted-foreground`   | `text-gray-500`         |
| Blue buttons / links   | `bg-primary`              | `bg-blue-500`           |
| Borders                | `border-border`           | `border-gray-200`       |
| Error / delete         | `text-destructive`        | `text-red-500`          |
| Success states         | `text-success`            | `text-green-600`        |
| Warning states         | `text-warning`            | `text-amber-500`        |

---

*End of UI Guidelines — v1.0*
