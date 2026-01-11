---
order: 11
icon: search
---
# Animations

### Implementing Animations with htmx and ASP.NET Core

Animations are essential for creating modern, polished web experiences. They provide visual feedback, guide user attention, and make state transitions feel natural rather than jarring. With htmx, you can achieve rich animations without complex JavaScript frameworks—just CSS transitions and a few strategic htmx attributes.

The key insight is that **htmx swaps are CSS-friendly**. When htmx swaps content, it temporarily adds CSS classes like `htmx-swapping` and `htmx-settling` to elements, giving you perfect hooks to trigger CSS animations and transitions at exactly the right moments.

#### Why Animations Matter

1. **Visual Continuity**: Smooth transitions help users understand what changed and where to look next
2. **Perceived Performance**: Well-timed animations can make your app feel faster by keeping users engaged during updates
3. **Error Feedback**: Shake animations or color flashes immediately communicate validation errors
4. **Professional Polish**: Thoughtful animations elevate a functional interface into a delightful user experience

#### Animation Patterns Covered

This section demonstrates eight practical animation techniques you can use in your htmx applications:

1. **Color Change**: Flash background colors to highlight updated content
2. **Fade Out on Swap**: Smoothly fade out old content before replacing it
3. **Fade In on Addition**: Gently introduce new elements to the page
4. **Slide and Expand**: Reveal content with smooth height and opacity transitions
5. **Crossfade Swap**: Blend between old and new content with overlapping fades
6. **Validation Shake**: Use attention-grabbing shake effects for form errors
7. **Indicator Morphing**: Transform loading indicators into final content
8. **Request in Flight (Settling)**: Animate content during htmx's "settling" phase

Each pattern leverages htmx's swap lifecycle and CSS transitions to create smooth, declarative animations with minimal code. You'll see how to combine `hx-swap` modifiers like `swap:500ms` and CSS classes to achieve professional results while keeping your logic server-side in C# and Razor Pages.